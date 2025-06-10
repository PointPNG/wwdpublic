using System.Numerics;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Maps;
using Content.Shared.VendingMachines;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Physics;

namespace Content.Shared.VendingMachines;

public sealed class TipOnMeleeHitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly AnchorableSystem _anchorable = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TipOnMeleeHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(EntityUid uid, TipOnMeleeHitComponent component, ref MeleeHitEvent args)
    {
        if (!args.IsHit || !_random.Prob(component.Chance))
            return;

        if (!TryComp(uid, out TransformComponent xform) || !xform.Anchored || xform.GridUid is not {} gridUid)
            return;

        if (!TryComp(gridUid, out MapGridComponent grid))
            return;

        var userPos = _transform.GetMapCoordinates(args.User).Position;
        var hitPos = _transform.GetMapCoordinates(uid).Position;
        var dir = args.Direction ?? (hitPos - userPos);
        if (dir == Vector2.Zero)
            return;

        var angle = Angle.FromWorldVec(dir);
        var offset = angle.GetCardinalDir().ToIntVec();
        var indices = grid.TileIndicesFor(xform.Coordinates) + offset;

        if (!_anchorable.TileFree(grid, indices))
            return;

        var coords = grid.GridTileToLocal(indices);
        _transform.Unanchor(uid, xform);
        _transform.SetCoordinates(uid, coords);

        if (TryComp<SpriteComponent>(uid, out var sprite))
            sprite.NoRotation = false;

        _transform.SetWorldRotation(uid, angle + Angle.FromDegrees(90));
    }
}
