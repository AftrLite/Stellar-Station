// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.Examine;
using Content.Stellar.Shared.Science.Components;

namespace Content.Stellar.Shared.Science;

public sealed class SharedStellarScienceBluespaceDriveSystem : EntitySystem
{

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarBluespaceDriveCapacitorComponent, ExaminedEvent>(OnCapacitorExamined);
        SubscribeLocalEvent<StellarBluespaceDriveCoreComponent, ExaminedEvent>(OnCoreExamined);
    }

    private void OnCapacitorExamined(Entity<StellarBluespaceDriveCapacitorComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("bsd-capacitor-examine-info"));
    }

    private void OnCoreExamined(Entity<StellarBluespaceDriveCoreComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("containment-capsule-info"));
    }
}
