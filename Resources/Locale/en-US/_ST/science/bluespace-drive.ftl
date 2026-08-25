# SPDX-FileCopyrightText: 2026 AftrLite
#
# SPDX-License-Identifier: LicenseRef-Wallening

### EXAMINES
bsd-capacitor-examine-info = Can be [color=#a73eff][bold]Charged[/bold][/color] using [color=#a73eff][bold]Containment Capsules[/bold][/color].
bsd-computer-examine-info = Can be [color=#a73eff][bold]Charged[/bold][/color] using [color=#a73eff][bold]Data Drives[/bold][/color].
bsd-core-examine-info = Can be [color=#a73eff][bold]Charged[/bold][/color] using [color=#a73eff][bold]Data Drives[/bold][/color].

bsd-core-examine-active = [color=#a73eff]The drive core is [bold]Active[/bold].[/color]
bsd-core-examine-charging = [color=#d4aa4b]The drive core is [bold]Recharging[/bold].[/color]
bsd-core-examine-ready = [color=#11b28e]The drive core is [bold]Charged[/bold] and ready.[/color]
bsd-core-examine-spooling = [color=#d4aa4b]The drive core [bold]Spooling[/bold] a departure sequence.[/color]


### POPUPS
bsd-popup-charge-gain-normal = The Bluespace Drive gains some charge!
bsd-popup-charge-gain-overcharge = The Bluespace Drive gains a lot of charge!
bsd-popup-charge-gain-hypercharge = The Bluespace Drive gains an incredible amount of charge!

bsd-popup-ready = The Bluespace Drive is charged and ready!
bsd-popup-spooling = The Bluespace Drive begins spooling...

### ANNOUNCEMENTS
announcement-bsd-sender = Drive Core
announcement-bsd-departure-brief = Spooling departure sequence. Hazard Sector departure in {$minutesandseconds}.
announcement-bsd-departure = Now spooling scheduled departure sequence. Hazard Sector departure in {$minutesandseconds}.
announcement-bsd-imminent = Bluespace jump imminent.
announcement-bsd-charge = {$progress ->
    [2] Bluespace Drive at 33% Charge.
    [3] Bluespace Drive at 66% Charge.
    [4] Bluespace Drive fully charged and ready.
     *[other] ERROR.
}
