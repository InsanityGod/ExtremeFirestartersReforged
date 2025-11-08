using BetterDeathMessages.Code;
using ExtremeFirestarters.Code.Behaviors;
using ExtremeFirestarters.Code.Config;
using InsanityLib.Attributes.Auto;
using Vintagestory.API.Common;

[assembly: AutoRegistry("extremefirestartersreforged")]
namespace ExtremeFirestarters;
public class ExtremeFirestarterReforgedModSystem : ModSystem
{
    public override void Start(ICoreAPI api)
    {
        base.Start(api);

        if(api.ModLoader.IsModEnabled("betterdeathmessages")) RegisterDeathMessagePool();
    }

    public static void RegisterDeathMessagePool() => DeathMessagePool.Pools.Add(new DeathMessagePool
    {
        PoolIdentifier = "firestarting-critical-failure",
        ManualAssignmentOnly = true,
        BaseCode = "extremefirestartersreforged:firestarting-critical-failure",
        Length = 4
    });

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
}