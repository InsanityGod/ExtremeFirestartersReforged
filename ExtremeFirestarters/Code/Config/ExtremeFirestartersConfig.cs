using InsanityLib.Attributes.Auto.Config;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ExtremeFirestarters.Code.Config;

public class ExtremeFirestartersConfig
{
    [AutoConfig("ExtremeFirestarterConfig.json", ServerSync = true)]
    public static ExtremeFirestartersConfig Instance { get; private set; }

    /// <summary>
    /// Multiplier on the speed of ignition attempts, increasing this value makes firestarting faster.
    /// </summary>
    [DefaultValue(1f)]
    [Range(0.1, 5)]
    public float IgnitionSpeedMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier on the chance of successful ignition attempts, increasing this value makes firestarting easier.
    /// </summary>
    [DefaultValue(1f)]
    [Range(0.1, 5)]
    public float SuccessChanceMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier on the chance of critical failures when failing to start a fire.
    /// Set to 0 to disable critical failures entirely.
    /// </summary>
    [DefaultValue(1f)]
    [Range(0, 10)]
    public float CriticalFailureChanceMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for the health damage inflicted when a critical failure occurs.
    /// NOTE: critical failures occur when a firestarting attempt fails AND a critical failure roll succeeds.
    /// Set to 0 to disable health damage entirely.
    /// </summary>
    [DefaultValue(1f)]
    [Range(0, 5)]
    public float CriticaFailureDamageMultiplier { get; set; } = 1f;

    /// <summary>
    /// Multiplier for the satiety cost of attempting to start a fire.
    /// Set to 0 to disable satiety cost entirely.
    /// </summary>
    [DefaultValue(1f)]
    [Range(0, 10)]
    public float SatietyCostMultiplier { get; set; } = 1f; //TODO maybe integration with stamina mods to consume stamina instead of/in addition to satiety?

    /// <summary>
    /// Wether or not firestarting attempts should automatically fail if the player's satiety is too low to cover the satiety cost.
    /// NOTE: this does not count as a failure for the purpose of critical failures and does not consume durability
    /// </summary>
    [DefaultValue(true)]
    public bool FailureOnLackOfSatiety { get; set; } = true;

    /// <summary>
    /// Multiplier on the durability firestarters have, increasing raises the max durability (changing this will not affect already damaged items).
    /// Set to 0 to make firestarters unbreakable.
    /// </summary>
    [DefaultValue(1f)]
    [Range(0, 10)]
    public float DurabilityMultiplier { get; set; } = 1f;

    /// <summary>
    /// Wether or not durability should be reduced even on failed firestarting attempts.
    /// </summary>
    [DefaultValue(true)]
    public bool DurabilityLossOnFailure { get; set; } = true;

    /// <summary>
    /// Wether or not the durability loss should happen after before the success chance is rolled instead of after, meaning you might fail due to running out of durability.
    /// NOTE: failing this way does not count as a failure for the purpose of critical failures.
    /// </summary>
    [DefaultValue(true)]
    public bool DurabilityLossBeforeSuccess { get; set; } = true;

    /// <summary>
    /// Enables/Disables the class traits added by this mod
    /// </summary>
    [DefaultValue(true)]
    public bool ClassTraits { get; set; } = true;
}