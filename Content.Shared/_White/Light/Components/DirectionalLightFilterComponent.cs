using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared._White.Light.Components;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class DirectionalLightFilterComponent : Component
{
    /// <summary>
    /// Direction relative to the entity's rotation that will block light.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Angle BlockAngle = Angle.Zero;

    /// <summary>
    /// Fraction of light blocked when viewed from the blocked direction (0-1).
    /// 1 means fully blocked, 0 means fully pass through.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float BlockFraction = 1f;
}
