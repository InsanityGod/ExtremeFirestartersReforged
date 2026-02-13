using BetterDeathMessages.Code;
using ExtremeFirestarters.Code.Behaviors;
using ExtremeFirestarters.Code.Config;
using ExtremeFirestarters.Code.Config.SubConfig;
using ExtremeFirestarters.Code.HarmonyPatches;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ExtremeFirestarters;

public partial class ExtremeFirestarterReforgedModSystem : ModSystem
{
    private ICoreClientAPI _capi;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        AutoSetup(api);
    }

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        if(api.ModLoader.IsModEnabled("betterdeathmessages")) RegisterDeathMessagePool();
    }

    partial void ManualPatches(Harmony harmony, ICoreAPI api)
    {
        //Load configs earlier then normal
        LoadAutoConfigs(api);

        var torchConfig = ExtremeFirestarterReforgedConfig.Instance.TorchConfig;
        if (torchConfig.ExtinguishIfNotHeld)
        {
            harmony.PatchCategory(nameof(TorchConfig.ExtinguishIfNotHeld));
            if (torchConfig.OnlyAllowPickupWithFreeHands) harmony.PatchCategory(nameof(TorchConfig.OnlyAllowPickupWithFreeHands));
        }
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        _capi = api;
        var torchConfig = ExtremeFirestarterReforgedConfig.Instance.TorchConfig;
        if (torchConfig.ExtinguishIfNotHeld && torchConfig.PreventAccidentalScrollExtinguish)
        {
            api.Event.BeforeActiveSlotChanged += PreventAccidentalExtinguishOnSlotChange;
        }
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        var torchConfig = ExtremeFirestarterReforgedConfig.Instance.TorchConfig;
        if (torchConfig.ExtinguishIfNotHeld)
        {
            api.Event.AfterActiveSlotChanged += ExtinguishTorchOnSlotChange;
        }
    }

    private static void ExtinguishTorchOnSlotChange(IServerPlayer player, ActiveSlotChangeEventArgs args)
    {
        var hotbar = player.InventoryManager.GetHotbarInventory();
        var fromSlot = hotbar[args.FromSlot];
        ExtinguishIfNotHeldPatches.TryExtinguishSlot(fromSlot, fromSlot?.Itemstack);
    }

    private EnumHandling PreventAccidentalExtinguishOnSlotChange(ActiveSlotChangeEventArgs args)
    {
        var invManger = _capi.World.Player.InventoryManager;
        var hotbar = invManger.GetHotbarInventory();
        var fromSlot = hotbar[args.FromSlot];
        if(fromSlot != invManger.OffhandHotbarSlot && fromSlot?.Itemstack?.Collectible is BlockTorch torch && torch.ExtinctVariant is not null && !torch.IsExtinct)
        {
            _capi.TriggerIngameError(this, "holding-active-torch", Lang.Get("extremefirestartersreforged:holding-active-torch"));
            return EnumHandling.PreventSubsequent;
        }

        return EnumHandling.PassThrough;
    }

    public static void RegisterDeathMessagePool() => DeathMessagePool.Pools.Add(new DeathMessagePool
    {
        PoolIdentifier = "firestarting-critical-failure",
        ManualAssignmentOnly = true,
        BaseCode = "extremefirestartersreforged:firestarting-critical-failure",
        Length = 4
    });

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        AutoAssetsLoaded(api);
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);

        foreach(var collectible in api.World.Collectibles)
        {
            if(collectible.Code?.Domain != "extremefirestartersreforged") continue;
            var fireStarterBehavior = collectible.GetBehavior<FireStarter>();
            if(fireStarterBehavior is null) continue;

            var mult = ExtremeFirestarterReforgedConfig.Instance.DurabilityMultiplier;
            collectible.Durability = mult > 0 ? (int)(collectible.Durability * mult) : 0;
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        if (_api is ICoreServerAPI sapi)
        {
            sapi.Event.AfterActiveSlotChanged -= ExtinguishTorchOnSlotChange;
        }
        else if (_api is ICoreClientAPI capi)
        {
            capi.Event.BeforeActiveSlotChanged += PreventAccidentalExtinguishOnSlotChange;
        }

        AutoDispose();
    }
}