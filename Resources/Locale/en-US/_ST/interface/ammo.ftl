# SPDX-FileCopyrightText: 2026 AftrLite
#
# SPDX-License-Identifier: LicenseRef-Wallening

## UI
stellar-ammo-ui-display = [font size=14][bold]{$count}[/bold][/font][font size=9]/{$total}[/font]

## Ammo Suffixes
stellar-ammo-suffix-ammo = Ammo
stellar-ammo-suffix-cells = {$count ->
    [1] Cell
    *[other] Cells
}
stellar-ammo-suffix-slugs = {$count ->
    [1] Slug
    *[other] Slugs
}
stellar-ammo-suffix-shells = {$count ->
    [1] Shell
    *[other] Shells
}
stellar-ammo-suffix-fuel = Fuel
stellar-ammo-suffix-injection = Injection

## Regular Ammo Types
stellar-ammo-pistol = Pistol
stellar-ammo-shotgun = Shotgun
stellar-ammo-sniper = Sniper
stellar-ammo-submachine = SMG
stellar-ammo-revolver = Revolver
stellar-ammo-rifle = Rifle

stellar-ammo-energy-pistol = Energy Pistol
stellar-ammo-energy-shotgun = Energy Shotgun
stellar-ammo-energy-sniper = Energy Sniper
stellar-ammo-energy-submachine = Energy SMG
stellar-ammo-energy-revolver = Energy Revolver
stellar-ammo-energy-rifle = Energy Rifle

## Unique Ammo Types
stellar-ammo-echion = [color=#77d94f]Echion[/color]
stellar-ammo-phoron = [color=#b63cb6]Phoron[/color]

## Stuff
stellar-ammo-reloader-occupied = It's currently occupied!
stellar-ammo-type-examine = Provides [bold]{$ammo}[/bold] {$suffix}.
stellar-ammo-remaining-examine = [color=#d4aa4b][bold]{$count}[/bold][/color] {$suffix} available.

stellar-weapon-type-examine = Requires [bold]{$ammo}[/bold] {$suffix}.
stellar-reloadable-ammo-examine = {$count ->
    [1] [color=#d4aa4b][bold]{$count}[/bold][/color] shot left.
    *[other] [color=#d4aa4b][bold]{$count}[/bold][/color] shots left.
}
