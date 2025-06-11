using Robust.Shared.GameStates;
using Robust.Shared.Containers;

namespace Content.Shared.Transport;

/// <summary>
/// Base component for generic transport entities that can hold passengers and items.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TransportComponent : Component
{
    /// <summary>
    /// Maximum number of passenger entities that can occupy this transport.
    /// </summary>
    [DataField]
    public int MaxPassengers = 1;

    /// <summary>
    /// Maximum number of items that can be stored in this transport.
    /// </summary>
    [DataField]
    public int MaxItemSlots = 0;

    /// <summary>
    /// Whether an ignition key is required for operation.
    /// </summary>
    [DataField]
    public bool RequireKey = true;

    /// <summary>
    /// Identifier of the key required to operate this transport.
    /// </summary>
    [DataField]
    public string? KeyId;

    [ViewVariables]
    public Container PassengerContainer = default!;

    [ViewVariables]
    public Container ItemContainer = default!;

    [ViewVariables]
    public ContainerSlot KeyContainer = default!;

    [DataField]
    public string PassengerContainerId = "passengers";

    [DataField]
    public string ItemContainerId = "storage";

    [DataField]
    public string KeyContainerId = "key_slot";
}
