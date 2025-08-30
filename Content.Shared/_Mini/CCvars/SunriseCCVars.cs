using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared._Mini.MiniCCVars;

public sealed partial class MiniCCVars : CVars
{
    /*
     * Radio chat icons
     */

    public static readonly CVarDef<bool> ChatIconsEnable =
        CVarDef.Create("chat_icon.enable", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /*
     * Pointing chat visuals
     */

    public static readonly CVarDef<bool> ChatPointingVisuals =
        CVarDef.Create("chat_icon_pointing.enable", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /*
     * Mute new ghost role sound
     */

    public static readonly CVarDef<bool> MuteGhostRoleNotification =
        CVarDef.Create("ghost.mute_role_notification", false, CVar.CLIENTONLY | CVar.ARCHIVE);
    /*
     * AntiSpam params
     */
    public static readonly CVarDef<bool> AntiSpamEnable =
        CVarDef.Create("anti_spam.enable", false, CVar.SERVER | CVar.ARCHIVE);
    public static readonly CVarDef<int> AntiSpamCounterShort =
        CVarDef.Create("anti_spam.counter_short", 1, CVar.SERVER | CVar.ARCHIVE);
    public static readonly CVarDef<int> AntiSpamCounterLong =
        CVarDef.Create("anti_spam.counter_long", 2, CVar.SERVER | CVar.ARCHIVE);
    public static readonly CVarDef<float> AntiSpamMuteDuration =
        CVarDef.Create("anti_spam.mute_duration", 10f, CVar.SERVER | CVar.ARCHIVE);
    public static readonly CVarDef<float> AntiSpamTimeShort =
        CVarDef.Create("anti_spam.time_short", 1.5f, CVar.SERVER | CVar.ARCHIVE);
    public static readonly CVarDef<float> AntiSpamTimeLong =
        CVarDef.Create("anti_spam.time_long", 5f, CVar.SERVER | CVar.ARCHIVE);
    /*
     * Chat sanitization
     */

    /// <summary>
    /// Включена ли санитизация чата (антиспам от набегаторов)
    /// </summary>
    public static readonly CVarDef<bool> ChatSanitizationEnable =
        CVarDef.Create("chatsan.enable", true, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// Контроллирует поведение санитизации.
    /// Агрессивное: если сообщение не проходит критерии - блокировать полностью его.
    /// Обычное: в сообщении, которое не проходит критерии, удалять не проходящие критерии части.
    /// </summary>
    public static readonly CVarDef<bool> ChatSanitizationAggressive =
        CVarDef.Create("chatsan.aggressive", true, CVar.SERVER | CVar.ARCHIVE);

    public static readonly CVarDef<bool> TracesEnabled =
        CVarDef.Create("opt.traces_enabled", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> HoldLookUp =
        CVarDef.Create("scope.hold_look_up", true, CVar.CLIENT | CVar.ARCHIVE);
}
