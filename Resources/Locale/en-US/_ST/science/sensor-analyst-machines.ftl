# SPDX-FileCopyrightText: 2026 AftrLite
#
# SPDX-License-Identifier: LicenseRef-Wallening

sensortower-names-dataset-1 = Alpha
sensortower-names-dataset-2 = Beta
sensortower-names-dataset-3 = Gamma
sensortower-names-dataset-4 = Delta
sensortower-names-dataset-5 = Epsilon
sensortower-names-dataset-6 = Omicron
sensortower-names-dataset-7 = Iota
sensortower-names-dataset-8 = Lambda
sensortower-names-dataset-9 = Kappa
sensortower-names-dataset-10 = Zeta
sensortower-names-dataset-11 = Eta
sensortower-names-dataset-12 = Rho
sensortower-names-dataset-13 = Mu
sensortower-names-dataset-14 = Nu
sensortower-names-dataset-15 = Tau

name-format-sensortower = sensor tower "{$part0}"


### EXAMINES
sensortower-examine-damaged = [color=#d60e4a]It needs [bold]Welding![/bold][/color]

sensortower-examine-driveempty = [color=#d60e4a]The tower requires an [bold]Empty Data Drive[/bold].[/color]
sensortower-examine-drivefull = [color=#d4aa4b]The tower's data drive is [bold]Full[/bold].[/color]
sensortower-examine-idle = [color=#d4aa4b]The tower is [bold]Ready[/bold] to synchronize.[/color]
sensortower-examine-processing = [color=#11b28e]The tower is [bold]Processing[/bold].[/color]
sensortower-examine-ringing = [color=#d4aa4b]The tower is awaiting [bold]Confirmation[/bold] from the main terminal.[/color]
sensortower-examine-syncing = [color=#d4aa4b]The tower is awaiting [bold]Sync Code[/bold] input.[/color]

sensorterminal-examine-idle = [color=#11b28e]The terminal is [bold]Ready[/bold] to use.[/color]
sensorterminal-examine-active = [color=#d4aa4b]The terminal has an active [bold]Sync Code[/bold].[/color]
sensorterminal-examine-ringing = [color=#d60e4a]A sensor tower is requesting a [bold]Sync Code[/bold] from this terminal![/color]

datadrive-examine-empty = [color=#d60e4a]It's [bold]Empty[/bold]. Plug it into a [bold]Sensor Tower[/bold].[/color]
datadrive-examine-full = [color=#11b28e]It's [bold]Full[/bold] and ready for processing![/color]

### POPUPS

sensortower-popup-requestsync = Requesting a sync code from the sensor network!
sensortower-popup-syncstarted = Synchronization initiated!
sensortower-popup-synccancelled = Synchronization cancelled!
sensortower-popup-synced = Synchronized!
sensortower-popup-failed = Synchronization failed!
sensortower-popup-timeout = Synchronization timeout.
sensortower-popup-drivefull = That data drive is full!

sensortower-popup-desync = Desynchronized!
sensortower-popup-desync-damage = Desynchronized due to damage!

sensortower-popup-nodrive = It needs an empty data drive in order to function.
sensortower-popup-ringing = I need to wait for the main terminal to generate a sync code.
sensortower-popup-processing = It's processing! I should come back later.
sensortower-popup-occupied = The main terminal is occupied.

sensorterminal-popup-requestsync = A sensor tower is requesting synchronization!
sensorterminal-popup-synccancelled = "Sync aborted by connected tower!"
sensorterminal-popup-synced = Sensor tower synced!
sensorterminal-popup-failed = Sensor tower failed to sync!
