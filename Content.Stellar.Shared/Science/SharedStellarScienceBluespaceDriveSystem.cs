// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Stellar.Shared.Science.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Stellar.Shared.Science;

public abstract class SharedStellarScienceBluespaceDriveSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StellarBluespaceDriveCapacitorComponent, InteractUsingEvent>(OnCapacitorInteract);
        SubscribeLocalEvent<StellarBluespaceDriveConsoleComponent, InteractUsingEvent>(OnConsoleInteract);

        SubscribeLocalEvent<StellarBluespaceDriveCapacitorComponent, ExaminedEvent>(OnCapacitorExamined);
        SubscribeLocalEvent<StellarBluespaceDriveCoreComponent, ExaminedEvent>(OnCoreExamined);
    }

    private void OnCapacitorInteract(Entity<StellarBluespaceDriveCapacitorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !TryComp<StellarContainmentCapsuleComponent>(args.Used, out var capsuleComp) || capsuleComp.StoredEnergy < 1)
            return;

        if (TryComp<StellarBluespaceDrivePartComponent>(ent, out var partComp) && partComp.LinkedCore != null)
        {
            _appearance.SetData(args.Used, StellarCapsuleVisuals.Visuals, 3);
            _audio.PlayLocal(partComp.SoundCharge, ent, args.User);
            args.Handled = true;

            var ev = new StellarDriveChargeEvent(Energy: capsuleComp.StoredEnergy);
            RaiseLocalEvent(partComp.LinkedCore.Value, ref ev, true);
            RemComp<StellarContainmentCapsuleComponent>(args.Used);
        }
    }

    private void OnConsoleInteract(Entity<StellarBluespaceDriveConsoleComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !TryComp<StellarDataDriveComponent>(args.Used, out var driveComp) || !driveComp.Full)
            return;

        if (TryComp<StellarBluespaceDrivePartComponent>(ent, out var partComp) && partComp.LinkedCore != null)
        {
            _appearance.SetData(args.Used, StellarDataDriveVisuals.Visuals, 3);
            _audio.PlayLocal(partComp.SoundCharge, ent, args.User);
            args.Handled = true;

            var ev = new StellarDriveChargeEvent(Data: 1);
            RaiseLocalEvent(partComp.LinkedCore.Value, ref ev, true);
            RemComp<StellarDataDriveComponent>(args.Used);
        }
    }

    private void OnCapacitorExamined(Entity<StellarBluespaceDriveCapacitorComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString("bsd-capacitor-examine-info"));
    }

    private void OnCoreExamined(Entity<StellarBluespaceDriveCoreComponent> ent, ref ExaminedEvent args)
    {
        switch (ent.Comp.DriveState)
        {
            case StellarBluespaceDriveState.Active:
                args.PushMarkup(Loc.GetString("bsd-core-examine-active"));
                break;
            case StellarBluespaceDriveState.Charging:
                args.PushMarkup(Loc.GetString("bsd-core-examine-charging"));
                break;
            case StellarBluespaceDriveState.Ready:
                args.PushMarkup(Loc.GetString("bsd-core-examine-ready"));
                break;
            case StellarBluespaceDriveState.Spooling:
                args.PushMarkup(Loc.GetString("bsd-core-examine-spooling"));
                break;
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class StellarDriveCountdownDoAfter : SimpleDoAfterEvent;

[ByRefEvent]
public readonly record struct StellarDriveCrisisEvent(bool Active);

[ByRefEvent]
public readonly record struct StellarDriveChargeEvent(int Data = 0, int Energy = 0);
