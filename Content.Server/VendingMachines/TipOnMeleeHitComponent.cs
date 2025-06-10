namespace Content.Server.VendingMachines;

/// <summary>
/// Added to vending machines that can tip over when struck in melee.
/// </summary>
[RegisterComponent]
public sealed partial class TipOnMeleeHitComponent : Component
{
    /// <summary>
    ///     Base probability of tipping when hit.
    /// </summary>
    [DataField("baseChance")] public float BaseChance = 0.05f;

    /// <summary>
    ///     Additional chance added after every failed hit.
    /// </summary>
    [DataField("increment")] public float Increment = 0.05f;

    /// <summary>
    ///     Current tipping chance. Increases with each hit until the machine falls over.
    /// </summary>
    [ViewVariables]
    public float CurrentChance;
}
