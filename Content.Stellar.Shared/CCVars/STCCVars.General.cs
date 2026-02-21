// SPDX-FileCopyrightText: 2026 TheShuEd
//
// SPDX-License-Identifier: LicenseRef-CosmicCult

using Robust.Shared.Configuration;

namespace Content.Stellar.Shared.CCVars;

public sealed partial class STCCVars
{
    // STELLAR-SPECIFIC STATION "WAKEUP"
    // Used so regular arrivals shit can stay disabled easily
    public static readonly CVarDef<bool> StationWakeupEnabled =
        CVarDef.Create("st_station_wakeup.enabled", false, CVar.SERVER);

    // How long the station takes to "wake up", aka for all the lights to turn on.
    // Also used by Hazard Sectors to set Bluespace Travel Time.
    public static readonly CVarDef<float> StationWakeupTime =
        CVarDef.Create("st_station_wakeup.ftl_time", 90f, CVar.SERVER);

    // Maximum amount of time crew is forced to sleep for at roundstart.
    public static readonly CVarDef<float> StationSleepTime =
        CVarDef.Create("st_station_wakeup.sleep_time", 15f, CVar.SERVER);
}
