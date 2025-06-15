using System.Numerics;
using Content.Client.Overlays;
using Content.Shared._White.Light;
using Content.Shared._White.Light.Components;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Maths;

namespace Content.Client._White.Light;

public sealed class DirectionalLightFilterSystem : SharedDirectionalLightFilterSystem
{
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    private ColorTintOverlay _overlay = default!;
    private EntityQuery<DirectionalLightFilterComponent, TransformComponent> _query;

    public override void Initialize()
    {
        base.Initialize();
        _query = GetEntityQuery<DirectionalLightFilterComponent, TransformComponent>();
        _overlay = new ColorTintOverlay
        {
            TintColor = Vector3.Zero,
            TintAmount = 0f
        };
        _overlayMan.AddOverlay(_overlay);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay(_overlay);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        var player = _playerMan.LocalEntity;
        if (player == null)
        {
            _overlay.TintAmount = 0f;
            return;
        }

        var playerPos = _entMan.GetComponent<TransformComponent>(player.Value).WorldPosition;
        var block = 0f;

        foreach (var (comp, xform) in _query)
        {
            var diff = playerPos - xform.WorldPosition;
            if (diff == Vector2.Zero)
                continue;

            var normal = (xform.WorldRotation + comp.BlockAngle).RotateVec(Vector2.UnitX);
            if (Vector2.Dot(diff, normal) > 0f)
                block = MathF.Max(block, comp.BlockFraction);
        }

        _overlay.TintAmount = block;
    }
}
