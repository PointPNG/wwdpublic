using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Content.Shared.Audio;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;

namespace Content.Shared.Transport;

/// <summary>
///     Handles common initialization for <see cref="TransportComponent"/>.
/// </summary>
public abstract class SharedTransportSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedBuckleSystem _buckle = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSound = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TransportComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TransportComponent, StrappedEvent>(OnStrapped);
        SubscribeLocalEvent<TransportComponent, UnstrappedEvent>(OnUnstrapped);
        SubscribeLocalEvent<TransportComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<TransportComponent, EntRemovedFromContainerMessage>(OnContainerModified);
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

        var seatIndex = 0;
        if (component.Driver == null)
        {
            component.Driver = args.Buckle.Owner;
            if (HasValidKey(uid, component))
                _mover.SetRelay(args.Buckle.Owner, uid);
        }
        else
        {
            seatIndex = 1;
        }

        if (seatIndex < component.SeatOffsets.Count)
        {
            var xform = Transform(args.Buckle.Owner);
            xform.LocalPosition = component.SeatOffsets[seatIndex];
        }
    }

    private void OnUnstrapped(EntityUid uid, TransportComponent component, ref UnstrappedEvent args)
    {
        if (args.Strap.Owner != uid)
            return;

        _containers.Remove(args.Buckle.Owner, component.PassengerContainer);

        if (component.Driver == args.Buckle.Owner)
        {
            RemComp<RelayInputMoverComponent>(component.Driver.Value);
            component.Driver = null;
        }
    }

    private void OnContainerModified(EntityUid uid, TransportComponent component, ContainerModifiedMessage args)
    {
        if (args.Container.ID != component.KeyContainerId)
            return;

        if (component.KeyContainer.ContainedEntity != null)
        {
            component.EngineOn = true;
            _ambientSound.SetAmbience(uid, true);

            if (component.Driver != null)
                _mover.SetRelay(component.Driver.Value, uid);
        }
        else
        {
            component.EngineOn = false;
            _ambientSound.SetAmbience(uid, false);

            if (component.Driver != null)
                RemComp<RelayInputMoverComponent>(component.Driver.Value);
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
