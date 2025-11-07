using Vintagestory.API.Common;

namespace ExtremeFirestarters.Code.ItemClasses;

public class MergedDurability : Item
{

    public override int GetMergableQuantity(ItemStack sinkStack, ItemStack sourceStack, EnumMergePriority priority)
    {
        if (priority == EnumMergePriority.DirectMerge && sinkStack.Collectible == sourceStack.Collectible)
        {
            int quantitySink = sinkStack.Item.GetRemainingDurability(sinkStack);
            int quantitySource = sourceStack.Item.GetRemainingDurability(sourceStack);
            int totalQuanity = quantitySink + quantitySource;
            int maxQuanity = sinkStack.Item.GetMaxDurability(sinkStack);
            
            if (totalQuanity <= maxQuanity) return 1;
        }

        return base.GetMergableQuantity(sinkStack, sourceStack, priority);
    }

    public override void TryMergeStacks(ItemStackMergeOperation op)
    {
        if (op.CurrentPriority == EnumMergePriority.DirectMerge)
        {
            ItemStack sinkStack = op.SinkSlot.Itemstack;
            ItemStack sourceStack = op.SourceSlot.Itemstack;

            if (sinkStack.Collectible == sourceStack.Collectible)
            {
                int quantitySink = sinkStack.Item.GetRemainingDurability(sinkStack);
                int quantitySource = sourceStack.Item.GetRemainingDurability(sourceStack);
                int totalQuanity = quantitySink + quantitySource;
                int maxQuanity = sinkStack.Item.GetMaxDurability(sinkStack);
                if (totalQuanity <= maxQuanity)
                {
                    op.SinkSlot.Itemstack.Item.SetDurability(sinkStack, totalQuanity);
                    op.SourceSlot.TakeOutWhole();
                    op.SinkSlot.MarkDirty();
                    op.SourceSlot.MarkDirty();
                    return;
                }
            }
        }

        base.TryMergeStacks(op);
    }
}
