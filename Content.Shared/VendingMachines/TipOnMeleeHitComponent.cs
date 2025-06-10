using Robust.Shared.GameStates;

namespace Content.Shared.VendingMachines;

/// <summary>
/// Added to vending machines that can tip over when struck in melee.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(TipOnMeleeHitSystem))]
public sealed partial class TipOnMeleeHitComponent : Component
{
    /// <summary>
    /// Probability of tipping when hit.
    /// </summary>
    [DataField("chance")]
    public float Chance = 0.25f;
}
