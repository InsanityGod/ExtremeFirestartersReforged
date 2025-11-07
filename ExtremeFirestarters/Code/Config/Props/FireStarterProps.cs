using System;
using System.ComponentModel.DataAnnotations;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ExtremeFirestarters.Code.Config.Props;

public class FireStarterProps
{
    /// <summary>
    /// Animation to be used while attempting to ignite a block.
    /// </summary>
    public string IgniteAnimation { get; set; } = "startfire";

    /// <summary>
    /// The sound to play while trying to ignite a block.
    /// </summary>
    public AssetLocation IgniteSound { get; set; } = new("game", "sounds/player/handdrill");

    //TODO dissalow igniting blocks if satiety is below lowest potential value
    /// <summary>
    /// The cost in satiety of trying to ignite a block.
    /// </summary>
    public NatFloat SatietyCost { get; set; } = new(7.5f, 2.5f, EnumDistribution.UNIFORM);

    /// <summary>
    /// The damage taken when a critical failure occurs.
    /// </summary>
    public NatFloat DamageOnCriticalFailure { get; set; } = new(1f, 1f, EnumDistribution.UNIFORM);

    /// <summary>
    /// How high the chance of a critical failure is when failing to ignite a block.
    /// </summary>
    [Range(0f, 1f)]
    public float CriticalFailureChance { get; set; } = 0.025f;

    /// <summary>
    /// The base speed of ignition attempts (relative to base game)
    /// </summary>
    public float IgnitionSpeed { get; set; } = 1f;

    /// <summary>
    /// The base chance of successfully igniting a block
    /// </summary>
    public float SuccessChance { get; set; } = 0.5f;

    public BlockDropItemStack[] ReturnOnBreak { get; set; } = [];
}
