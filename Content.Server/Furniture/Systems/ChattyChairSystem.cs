using Content.Server.Chat.Systems;
using Content.Server.Furniture.Components;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Mobs.Components;
using Content.Shared.Traits.Assorted.Components;
using System.Collections.Generic;
using Robust.Shared.Collections;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Furniture.Systems;

public sealed class ChattyChairSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private readonly HashSet<Entity<MobStateComponent>> _nearby = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChattyChairComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandRecipients);
    }

    private void OnStartup(Entity<ChattyChairComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextTime = _random.NextFloat(ent.Comp.MinInterval, ent.Comp.MaxInterval);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_cfg.GetCVar(CCVars.ChattyChairsEnabled))
            return;

        var query = EntityQueryEnumerator<ChattyChairComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var chair, out var xform))
        {
            chair.Accumulator += frameTime;
            if (chair.Accumulator < chair.NextTime)
                continue;

            // Check for any nearby mobs
            _nearby.Clear();
            var coordinates = xform.Coordinates;
            _lookup.GetEntitiesInRange<MobStateComponent>(coordinates, 2f, _nearby, LookupFlags.Approximate | LookupFlags.Dynamic);
            bool found = _nearby.Count > 0;

            if (!found || chair.Lines.Count == 0)
                continue;

            chair.Accumulator = 0f;
            chair.NextTime = _random.NextFloat(chair.MinInterval, chair.MaxInterval);

            var line = Loc.GetString(_random.Pick(chair.Lines));
            _chat.TrySendInGameICMessage(uid, line, InGameICChatType.Speak, hideChat: true, ignoreActionBlocker: true);
        }
    }

    private void OnExpandRecipients(ExpandICChatRecipientsEvent ev)
    {
        if (!HasComp<ChattyChairComponent>(ev.Source))
            return;

        var remove = new ValueList<ICommonSession>(ev.Recipients.Count);
        foreach (var (session, _) in ev.Recipients)
        {
            if (session.AttachedEntity is not { Valid: true } ent)
                continue;

            if (!HasComp<ChairSpeakerComponent>(ent))
                remove.Add(session);
        }

        foreach (var session in remove.Span)
            ev.Recipients.Remove(session);
    }
}
