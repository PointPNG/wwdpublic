using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server.VendingMachines;

/// <summary>
/// Added to vending machines that can tip over when struck in melee.
/// </summary>
[RegisterComponent]
[Access(typeof(TipOnMeleeHitSystem))]
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

    /// <summary>
    /// Chance for the machine to spit out a random item when it falls.
    /// </summary>
    [DataField("spillChance")] public float SpillChance = 0.5f;

    /// <summary>
    ///     Sound played when the machine falls over.
    /// </summary>
    [DataField("fallSound")]
    public SoundSpecifier FallSound = new SoundPathSpecifier("/Audio/Effects/bodyfall4.ogg");

    /// <summary>
    ///     Whether the machine has already fallen over once.
    /// </summary>
    [ViewVariables]
    public bool HasFallen;
}
