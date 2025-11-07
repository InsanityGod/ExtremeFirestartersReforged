using ExtremeFirestarters.Code.Config;
using ExtremeFirestarters.Code.Config.Props;
using InsanityLib.Util;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;


namespace ExtremeFirestarters.Code.Behaviors;

public class FireStarter(CollectibleObject collObj) : CollectibleBehavior(collObj)
{
    public FireStarterProps Props { get; protected set; }
    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);

        foreach(var drop in Props.ReturnOnBreak)
        {
            drop.Resolve(api.World, "FireStarter block drops of ", collObj.Code);
        }
    }
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        Props = properties.AsObject<FireStarterProps>();
    }

    public void OnCriticalFailure(EntityAgent entity)
    {
        if(entity?.Api.Side != EnumAppSide.Server) return;
        var damage = Props.DamageOnCriticalFailure.nextFloat() * ExtremeFirestartersConfig.Instance.CriticaFailureDamageMultiplier;

        if(damage <= 0f) return;
        //TODO better death messages support
        entity.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Internal,
            Type = EnumDamageType.Injury,
            SourceEntity = entity
        }, damage);
    }

    /// <returns>false if item broke due to durability loss, else true</returns>
    public bool ConsumeDurability(ItemSlot slot, EntityAgent byEntity)
    {
        if(slot.Itemstack is null) return false;
        collObj.DamageItem(byEntity.World, byEntity, slot, 1);

        var result = slot.Itemstack is not null;

        if (!result)
        {
            foreach(var drop in Props.ReturnOnBreak)
            {
                var stack = drop.GetNextItemStack();
                if(stack is null) continue;
                if (!byEntity.TryGiveItemStack(stack))
                {
                    byEntity.World.SpawnItemEntity(stack, byEntity.ServerPos.XYZ);
                }
                if(drop.LastDrop) break;
            }
        }

        return result;
    }

    public float GetRandomSatietyCost(EntityAgent byEntity) => Props.SatietyCost.nextFloat() * ExtremeFirestartersConfig.Instance.SatietyCostMultiplier;
    
    public float CalculateIgnitionSpeed(EntityAgent byEntity) => Props.IgnitionSpeed * ExtremeFirestartersConfig.Instance.IgnitionSpeedMultiplier * byEntity.Stats.GetBlended("firestartingSpeed");

    public float CalculateSuccessChance(EntityAgent byEntity) => Props.SuccessChance * ExtremeFirestartersConfig.Instance.SuccessChanceMultiplier * byEntity.Stats.GetBlended("firestartingSuccess");

    public float CalculateCriticalFailureChance(EntityAgent byEntity) => Props.CriticalFailureChance * ExtremeFirestartersConfig.Instance.CriticalFailureChanceMultiplier;

    public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
    {
        var playerEntity = byEntity as EntityPlayer;
        if (blockSel?.Position is null 
            || playerEntity is not null && !playerEntity.Api.World.Claims.TryAccess(playerEntity.Player, blockSel.Position, EnumBlockAccessFlags.Use) 
            || blockSel.GetOrFindBlock(byEntity.Api.World) is not IIgnitable ignitable) return;

        handHandling = EnumHandHandling.PreventDefault;
        handling = EnumHandling.PreventSubsequent;
        if(ignitable.OnTryIgniteBlock(byEntity, blockSel.Position, 0) < EnumIgniteState.Ignitable) return;

        byEntity.AnimManager.StartAnimation(Props.IgniteAnimation);
        byEntity.World.PlaySoundAt(Props.IgniteSound, byEntity, playerEntity?.Player, false, 16);
    }

    public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        var playerEntity = byEntity as EntityPlayer;
        if (blockSel?.Position is null 
            || playerEntity is not null && !playerEntity.Api.World.Claims.TryAccess(playerEntity.Player, blockSel.Position, EnumBlockAccessFlags.Use) 
            || blockSel.GetOrFindBlock(byEntity.Api.World) is not IIgnitable ignitable) return true;

        handling = EnumHandling.PreventSubsequent;

        float scaledSecondsUsed = secondsUsed * CalculateIgnitionSpeed(byEntity);
        var ignitionState = ignitable.OnTryIgniteBlock(byEntity, blockSel.Position, scaledSecondsUsed);
        if(ignitionState < EnumIgniteState.Ignitable) return false;

        if (byEntity.World is IClientWorldAccessor clientWorld)
        {
            ModelTransform modelTransform = new();
            modelTransform.EnsureDefaultValues();
            float num = GameMath.Clamp(1f - 2f * scaledSecondsUsed, 0f, 1f);
            Random rand = clientWorld.Rand;
            modelTransform.Translation.Set(num * num * num * 1.6f - 1.6f, 0f, 0f);
            modelTransform.Rotation.Y = 0f - Math.Min(scaledSecondsUsed * 120f, 30f);
            if (scaledSecondsUsed > 0.5f)
            {
                modelTransform.Translation.Add((float)rand.NextDouble() * 0.1f, (float)rand.NextDouble() * 0.1f, (float)rand.NextDouble() * 0.1f);
                clientWorld.SetCameraShake(0.04f);
            }
        }
        return ignitionState == EnumIgniteState.Ignitable;
    }

    public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandling handling)
    {
        byEntity.AnimManager.StopAnimation(Props.IgniteAnimation);

        var playerEntity = byEntity as EntityPlayer;
        if (blockSel?.Position is null 
            || playerEntity is not null && !playerEntity.Api.World.Claims.TryAccess(playerEntity.Player, blockSel.Position, EnumBlockAccessFlags.Use) 
            || blockSel.GetOrFindBlock(byEntity.Api.World) is not IIgnitable ignitable) return;

        handling = EnumHandling.PreventSubsequent;

        if (byEntity.World.Side == EnumAppSide.Client) return;

        var scaledSecondsUsed = secondsUsed * CalculateIgnitionSpeed(byEntity);
        if(ignitable.OnTryIgniteBlock(byEntity, blockSel.Position, scaledSecondsUsed) != EnumIgniteState.IgniteNow) return;

        var satietyCost = GetRandomSatietyCost(byEntity);
        if(satietyCost > 0 && byEntity.GetBehavior<EntityBehaviorHunger>() is EntityBehaviorHunger hunger)
        {
            var newSatiety = hunger.Saturation - satietyCost;
            hunger.Saturation = Math.Max(0, newSatiety);
            if(ExtremeFirestartersConfig.Instance.FailureOnLackOfSatiety && newSatiety <= 0) return;
        }

        if (ExtremeFirestartersConfig.Instance.DurabilityLossBeforeSuccess && !ConsumeDurability(slot, byEntity)) return;

        var successChance = CalculateSuccessChance(byEntity);
        if(successChance < byEntity.World.Rand.NextDouble())
        {
            if (CalculateCriticalFailureChance(byEntity) > byEntity.World.Rand.NextDouble()) OnCriticalFailure(byEntity);
            if (ExtremeFirestartersConfig.Instance.DurabilityLossOnFailure && !ExtremeFirestartersConfig.Instance.DurabilityLossBeforeSuccess) ConsumeDurability(slot, byEntity);
            return;
        }

        EnumHandling _ = EnumHandling.PassThrough;
        ignitable.OnTryIgniteBlockOver(byEntity, blockSel.Position, scaledSecondsUsed, ref _);
        if (!ExtremeFirestartersConfig.Instance.DurabilityLossBeforeSuccess) ConsumeDurability(slot, byEntity);
    }

    public override bool OnHeldInteractCancel(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, EnumItemUseCancelReason cancelReason, ref EnumHandling handled)
    {
        byEntity.AnimManager.StopAnimation(Props.IgniteAnimation);
        return true;
    }

    public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling) => [
        new WorldInteraction
        {
            HotKeyCode = "shift",
            ActionLangCode = "heldhelp-igniteblock",
            MouseButton = EnumMouseButton.Right
        }
    ];
}