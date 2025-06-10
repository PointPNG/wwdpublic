using System;
using System.Numerics;
using System.Collections.Generic;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Maps;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Mind.Components;
using Content.Server.Advertise;
using Content.Server.Advertise.Components;
using Content.Server.Chat.Systems;
using Content.Server.Stunnable;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Server.VendingMachines;

public sealed class TipOnMeleeHitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AnchorableSystem _anchorable = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SpeakOnUIClosedSystem _speak = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TipOnMeleeHitComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<TipOnMeleeHitComponent, AttackedEvent>(OnAttacked);
    }

    private void OnComponentInit(EntityUid uid, TipOnMeleeHitComponent component, ComponentInit args)
    {
        component.CurrentChance = component.BaseChance;
    }

    private void OnAttacked(EntityUid uid, TipOnMeleeHitComponent component, ref AttackedEvent args)
    {
        if (!_random.Prob(component.CurrentChance))
        {
            component.CurrentChance = MathF.Min(1f, component.CurrentChance + component.Increment);
            return;
        }

        component.CurrentChance = component.BaseChance;

        if (!TryComp(uid, out TransformComponent xform) || !xform.Anchored || xform.GridUid is not {} gridUid)
            return;

        if (!TryComp(gridUid, out MapGridComponent grid))
            return;

        var indices = grid.TileIndicesFor(xform.Coordinates);

        var candidates = new List<Vector2i>(4);
        var offsets = new[]
        {
            new Vector2i(0, 1),
            new Vector2i(0, -1),
            new Vector2i(1, 0),
            new Vector2i(-1, 0)
        };

        foreach (var off in offsets)
        {
            var tile = indices + off;
            if (_anchorable.TileFree(grid, tile))
                candidates.Add(off);
        }

        if (candidates.Count == 0)
            return;

        var offset = _random.Pick(candidates);
        var angle = Angle.FromWorldVec(new Vector2(offset.X, offset.Y));
        var destIndices = indices + offset;

        var coords = grid.GridTileToLocal(destIndices);
        _transform.Unanchor(uid, xform);
        _transform.SetCoordinates(uid, coords);
        _transform.SetWorldRotation(uid, angle + Angle.FromDegrees(90));
        _transform.AnchorEntity(uid, xform);

        if (TryComp<SpeakOnUIClosedComponent>(uid, out var speak))
            _speak.TrySpeak((uid, speak));

        var bounds = _lookup.GetWorldAABB(uid);
        foreach (var body in _physics.GetCollidingEntities(xform.MapID, bounds))
        {
            if (body.BodyType == BodyType.Static || body.Owner == uid || !body.Hard)
                continue;

            var blunt = _prototype.Index<DamageTypePrototype>("Blunt");
            _damageable.TryChangeDamage(body.Owner, new DamageSpecifier(blunt, 100), origin: uid);

            if (TryComp<MindContainerComponent>(body.Owner, out var mind) && mind.HasMind)
            {
                _chat.TryEmoteWithChat(body.Owner, "Scream");
                _stun.TryKnockdown(body.Owner, TimeSpan.FromSeconds(5), true);
            }
        }
    }
}
