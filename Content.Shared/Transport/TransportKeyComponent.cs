using Robust.Shared.GameStates;

namespace Content.Shared.Transport;

/// <summary>
/// Component placed on transport ignition keys to mark which transports they can operate.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TransportKeyComponent : Component
{
    /// <summary>
    /// Identifier matching <see cref="TransportComponent.KeyId"/>.
    /// </summary>
    [DataField]
    public string TransportId = string.Empty;
}
