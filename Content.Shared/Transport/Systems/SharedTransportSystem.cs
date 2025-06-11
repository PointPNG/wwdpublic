using System.Numerics;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Transport;

/// <summary>
///     Handles common initialization for <see cref="TransportComponent"/>.
/// </summary>
public abstract class SharedTransportSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TransportComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TransportComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<TransportComponent, UnstrappedEvent>(OnUnstrapped);
    }

    private void OnStartup(EntityUid uid, TransportComponent component, ComponentStartup args)
    {
        component.PassengerContainer = _containers.EnsureContainer<Container>(uid, component.PassengerContainerId);
        component.ItemContainer = _containers.EnsureContainer<Container>(uid, component.ItemContainerId);
        component.KeyContainer = _containers.EnsureContainer<ContainerSlot>(uid, component.KeyContainerId);
    }

    private void OnStrapped(EntityUid uid, TransportComponent component, ref StrappedEvent args)
    {
        if (args.Strap.Owner != uid)
            return;

        if (component.PassengerContainer.ContainedEntities.Count >= component.MaxPassengers)
        {
            _buckle.TryUnbuckle(args.Buckle.Owner, args.Buckle.Owner);
            return;
        }

        _containers.Insert(args.Buckle.Owner, component.PassengerContainer);
        UpdateSeatPositions(uid, component);
    }

    private void OnUnstrapped(EntityUid uid, TransportComponent component, ref UnstrappedEvent args)
    {
        if (args.Strap.Owner != uid)
            return;

        _containers.Remove(args.Buckle.Owner, component.PassengerContainer);
        UpdateSeatPositions(uid, component);
    }

    private void UpdateSeatPositions(EntityUid uid, TransportComponent component)
    {
        var i = 0;
        foreach (var passenger in component.PassengerContainer.ContainedEntities)
        {
            var offset = i < component.SeatOffsets.Count ? component.SeatOffsets[i] : Vector2.Zero;
            Transform(passenger).LocalPosition = offset;
            i++;
        }
    }

    /// <summary>
    ///     Checks whether the provided key entity can operate the specified transport.
    /// </summary>
    public bool IsKeyValid(EntityUid transport, EntityUid key, TransportComponent? transportComp = null,
        TransportKeyComponent? keyComp = null)
    {
        if (!Resolve(transport, ref transportComp) || !Resolve(key, ref keyComp))
            return false;

        if (string.IsNullOrEmpty(transportComp.KeyId))
            return true;

        return transportComp.KeyId == keyComp.TransportId;
    }

    /// <summary>
    ///     Returns true if the transport has a valid key inserted, or a key is not required.
    /// </summary>
    public bool HasValidKey(EntityUid transport, TransportComponent? comp = null)
    {
        if (!Resolve(transport, ref comp))
            return false;

        var key = comp.KeyContainer.ContainedEntity;

        if (key == null)
            return !comp.RequireKey;

        return IsKeyValid(transport, key.Value, comp);
    }
}
