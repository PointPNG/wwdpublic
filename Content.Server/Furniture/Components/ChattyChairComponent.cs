using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;

namespace Content.Server.Furniture.Components;

/// <summary>
/// Allows a chair to speak random lines when near a character.
/// </summary>
[RegisterComponent]
public sealed partial class ChattyChairComponent : Component
{
    /// <summary>
    /// Lines that can be said by the chair.
    /// </summary>
    [DataField]
    public List<LocId> Lines { get; set; } = new();

    /// <summary>
    /// Minimum time between lines in seconds.
    /// </summary>
    [DataField]
    public float MinInterval = 2f;

    /// <summary>
    /// Maximum time between lines in seconds.
    /// </summary>
    [DataField]
    public float MaxInterval = 5f;

    /// <summary>
    /// Accumulator since the last line was spoken.
    /// </summary>
    [DataField]
    public float Accumulator;

    /// <summary>
    /// Next time threshold to speak.
    /// </summary>
    [DataField]
    public float NextTime;
}
