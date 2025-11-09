using System.ComponentModel;

namespace ExtremeFirestarters.Code.Config.SubConfig;

public class TorchConfig
{
    /// <summary>
    /// Wether torches should be automatically extinguished if not held in hand/offhand (crafting grid is excluded)
    /// </summary>
    [DefaultValue(true)]
    public bool ExtinguishIfNotHeld { get; set; } = true;

    /// <summary>
    /// If enabled will prevent player from changing active hotbar slot while holding a turned on torch
    /// NOTE: Only applicable when ExtinguishIfNotHeld is enabled
    /// </summary>
    [DefaultValue(true)]
    public bool PreventAccidentalScrollExtinguish { get; set; } = true;
    
    /// <summary>
    /// If enabled torches can only be picked up in active hotbar slot and offhand slot
    /// NOTE: Only applicable when ExtinguishIfNotHeld is enabled
    /// </summary>
    [DefaultValue(true)]
    public bool OnlyAllowPickupWithFreeHands { get; set; } = true;
}
