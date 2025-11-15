using ExtremeFirestarters.Code.Config.SubConfig;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.Common;
using Vintagestory.GameContent;

namespace ExtremeFirestarters.Code.HarmonyPatches;

[HarmonyPatch]
[HarmonyPatchCategory(nameof(TorchConfig.ExtinguishIfNotHeld))]
public static class ExtinguishIfNotHeldPatches
{
    public static readonly AssetLocation ExtinguishSound = new("game", "sounds/effect/extinguish");

    [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.OnItemSlotModified))]
    [HarmonyPrefix]
    public static void ExtinguishFires(ItemSlot __instance, ItemStack sinkStack) => TryExtinguishSlot(__instance, sinkStack);

    public static void TryExtinguishSlot(ItemSlot slot, ItemStack itemstack)
    {
        var api = slot?.Inventory?.Api;

        if (api is null || itemstack?.Collectible is not BlockTorch torch || torch.ExtinctVariant is null || torch.IsExtinct || slot.GetType().Name == "ItemSlotGround") return;

        var pos = slot.Inventory.Pos;

        if (slot.Inventory is InventoryBasePlayer playerInventory)
        {
            var inventoryManager = playerInventory.Player.InventoryManager;
            if (playerInventory is InventoryCraftingGrid || slot == inventoryManager.ActiveHotbarSlot || slot == inventoryManager.OffhandHotbarSlot || slot == inventoryManager.MouseItemSlot) return;

            pos ??= playerInventory.Player.Entity.Pos.AsBlockPos;
        }

        if (pos is not null && api.Side == EnumAppSide.Server) api.World.PlaySoundAt(ExtinguishSound, pos, .5f, range: 16f);

        itemstack.Id = torch.ExtinctVariant.Id;
        itemstack.ResolveBlockOrItem(api.World);
    }
}
