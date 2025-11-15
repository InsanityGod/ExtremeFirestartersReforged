using ExtremeFirestarters.Code.Config.SubConfig;
using HarmonyLib;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace ExtremeFirestarters.Code.HarmonyPatches;
[HarmonyPatch]
[HarmonyPatchCategory(nameof(TorchConfig.OnlyAllowPickupWithFreeHands))]
public static class TorchPickupTweaks
{
    [HarmonyPatch(typeof(PlayerInventoryManager), nameof(PlayerInventoryManager.GetBestSuitedSlot), typeof(ItemSlot), typeof(bool), typeof(ItemStackMoveOperation), typeof(List<ItemSlot>))]
    [HarmonyPrefix]
    public static bool Prefix(PlayerInventoryManager __instance, ItemSlot sourceSlot, ref ItemSlot __result)
    {
        if(sourceSlot.Itemstack?.Collectible is not BlockTorch torch || torch.ExtinctVariant is null || torch.IsExtinct) return true;

        var slot = __instance.ActiveHotbarSlot;

        if (slot.CanTakeFrom(sourceSlot))
        {
            __result = slot;
            return false;
        }

        slot = __instance.OffhandHotbarSlot;

        if (slot.CanTakeFrom(sourceSlot))
        {
            __result = slot;
            return false;
        }
        __result = null;
        return false;
    }
}
