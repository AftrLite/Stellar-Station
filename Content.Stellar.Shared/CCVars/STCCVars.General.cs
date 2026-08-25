// SPDX-FileCopyrightText: 2026 AftrLite
//
// SPDX-License-Identifier: LicenseRef-Wallening

using Robust.Shared.Configuration;

namespace Content.Stellar.Shared.CCVars;

public sealed partial class STCCVars
{
    //  The default maximum amount of time, in MINUTES, that it takes for the bluespace drive takes to charge up.
    public static readonly CVarDef<float> DriveChargeTimeMax =
        CVarDef.Create("st_bluespace_drive.charge_time_max", 60f, CVar.SERVER);

    //  The default minimum amount of time, in MINUTES, that the bluespace drive can take to charge up.
    public static readonly CVarDef<float> DriveChargeTimeMin =
        CVarDef.Create("st_bluespace_drive.charge_time_min", 40f, CVar.SERVER);

    //  The default minimum amount of time, in MINUTES, that the bluespace begins automatically charging up to depart the sector (ENDING THE ROUND)
    // This value MUST exceed DriveChargeTimeMax.
    public static readonly CVarDef<float> DriveAutomaticLeaveTime =
        CVarDef.Create("st_bluespace_drive.automatic_leave_time", 85f, CVar.SERVER);

    // STELLAR-SPECIFIC STATION "WAKEUP"
    // Used so regular arrivals shit can stay disabled easily
    public static readonly CVarDef<bool> StationWakeupEnabled =
        CVarDef.Create("st_station_wakeup.enabled", true, CVar.SERVER); // TODO: Set this to TRUE when stellar enters playtesting, and set the development.toml have it as FALSE!

    // How long the station takes to "wake up", aka for all the lights to turn on. In SECONDS.
    // Also used by Hazard Sectors to set Bluespace Travel Time.
    public static readonly CVarDef<float> StationWakeupTime =
        CVarDef.Create("st_station_wakeup.ftl_time", 90f, CVar.SERVER);

    // Maximum amount of time crew is forced to sleep for at roundstart.
    public static readonly CVarDef<float> StationSleepTime =
        CVarDef.Create("st_station_wakeup.sleep_time", 15f, CVar.SERVER);

    // Amount of time, in SECONDS, a player must wait between "socials". Applies to Item Offers, Emotes, and Co-op Emotes.
    public static readonly CVarDef<float> SocialCooldownTime =
        CVarDef.Create("st_social.cooldown_time", 5f, CVar.SERVER | CVar.REPLICATED);

    // How close two players need to be in order to perform a collaborative social interaction.
    public static readonly CVarDef<float> SocialInteractionRange =
        CVarDef.Create("st_social.interaction_range", 0.5f, CVar.SERVER | CVar.REPLICATED);

    // The MINIMUM amount of players expected for regular gameplay.
    public static readonly CVarDef<int> MinExpectedPlayers =
        CVarDef.Create("st_game.min_expected_players", 15, CVar.SERVER);
}
