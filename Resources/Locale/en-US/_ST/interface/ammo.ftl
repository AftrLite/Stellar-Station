# SPDX-FileCopyrightText: 2026 AftrLite
#
# SPDX-License-Identifier: LicenseRef-Wallening

## UI
stellar-ammo-ui-display = [font size=14][bold]{$count}[/bold][/font][font size=9]/{$total}[/font]

## Ammo Suffixes
stellar-ammo-suffix-ammo = Ammo
stellar-ammo-suffix-capsules = {$count ->
    [1] Capsule
    *[other] Capsules
}
stellar-ammo-suffix-cartridges = {$count ->
    [1] Cartridge
    *[other] Cartridges
}
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
stellar-ammo-suffix-grenades = {$count ->
    [1] Grenade
    *[other] Grenades
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
stellar-ammo-chemical = [color=#538aa6]Chem-payload[/color]
stellar-ammo-tachyon = [color=#ba8df8]Tachyon[/color]

## INTERACTIONS/POPUPS
stellar-ammo-regen = +{$count} ammo!
stellar-ammo-reserves-empty = No reserves left!
stellar-ammo-magazine-empty = Empty!
stellar-ammo-reloader-occupied = It's currently occupied!
stellar-ammo-type-examine = Provides [bold]{$ammo}[/bold] {$suffix}.
stellar-ammo-remaining-examine = [color=#d4aa4b][bold]{$count}[/bold][/color] {$suffix} available.

stellar-weapon-type-examine = Requires [bold]{$ammo}[/bold] {$suffix}.
stellar-ammo-regen-examine = Regenerates [color=#d4aa4b][bold]{$ammo}[/bold][/color] ammo every [color=#d4aa4b][bold]{$count}[/bold][/color] seconds.
stellar-reloadable-ammo-examine = {$count ->
    [1] [color=#d4aa4b][bold]{$count}[/bold][/color] shot left. Press [color=#d4aa4b][keybind="StellarReloadGun"][/color] to reload.
    *[other] [color=#d4aa4b][bold]{$count}[/bold][/color] shots left. Press [color=#d4aa4b][keybind="StellarReloadGun"][/color] to reload.
}
