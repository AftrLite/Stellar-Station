// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

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

    // Amount of time, in seconds, a player must wait between "socials". Applies to Item Offers, Emotes, and Co-op Emotes.
    public static readonly CVarDef<float> SocialCooldownTime =
        CVarDef.Create("st_social.cooldown_time", 5f, CVar.SERVER | CVar.REPLICATED);

    // How close two players need to be in order to perform a collaborative social interaction.
    public static readonly CVarDef<float> SocialInteractionRange =
        CVarDef.Create("st_social.interaction_range", 0.5f, CVar.SERVER | CVar.REPLICATED);
}
