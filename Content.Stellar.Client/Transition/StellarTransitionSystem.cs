// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Stellar.Shared.Transition;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;

namespace Content.Stellar.Client.Transition;

public sealed class StellarTransitionSystem : SharedStellarTransitionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IUserInterfaceManager _uiMan = default!;

    private StellarTransitionUIController _transition = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<StellarTransitionEvent>(OnTransition);

        _transition = _uiMan.GetUIController<StellarTransitionUIController>();
    }

    private void OnTransition(StellarTransitionEvent args)
    {
        var entity = GetEntity(args.Target);

        if (entity != _player.LocalEntity || !_timing.IsFirstTimePredicted)
            return;

        _transition.StartTransition(_transition.IsClosed, args.Length);
    }

    public void ClientTransition(EntityUid entity, TimeSpan? duration = null)
    {
        if (entity != _player.LocalEntity || !_timing.IsFirstTimePredicted)
            return;

        _transition.StartTransition(_transition.IsClosed, duration);
    }

}
