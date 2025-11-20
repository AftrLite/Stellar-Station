// SPDX-FileCopyrightText: 2025 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Server.Body.Systems;
using Content.Stellar.Shared.Mobs;

namespace Content.Stellar.Server.Mobs;

public sealed class InternalsResourceBarSystem : SharedInternalsResourceBarSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InternalsResourceBarComponent, InhaleLocationEvent>(OnInhaleLocation);
    }

    private void OnInhaleLocation(Entity<InternalsResourceBarComponent> ent, ref InhaleLocationEvent args)
    {
        UpdateBars(ent);
    }
}
