using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Transport;

/// <summary>
///     Handles common initialization for <see cref="TransportComponent"/>.
/// </summary>
public abstract class SharedTransportSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TransportComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TransportComponent component, ComponentStartup args)
    {
        component.PassengerContainer = _containers.EnsureContainer<Container>(uid, component.PassengerContainerId);
        component.ItemContainer = _containers.EnsureContainer<Container>(uid, component.ItemContainerId);
        component.KeyContainer = _containers.EnsureContainer<ContainerSlot>(uid, component.KeyContainerId);
    }
}
