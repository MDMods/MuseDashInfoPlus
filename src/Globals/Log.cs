using MDIP.Presentation;

namespace MDIP.Globals;

// The one logger for the whole mod. Wraps MelonLoader's logger directly; no per-type generic and
// no injection — there is exactly one log sink, so a static is the honest shape.
internal static class Log
{
    public static void Info(object message) => Melon<MDIPMod>.Logger.Msg(Format(message));
    public static void Warn(object message) => Melon<MDIPMod>.Logger.Warning(Format(message));
    public static void Error(object message) => Melon<MDIPMod>.Logger.Error(Format(message));
    public static void Fatal(object message) => Melon<MDIPMod>.Logger.BigError(Format(message));

    private static string Format(object message) => message switch
    {
        null => "null",
        Exception ex => ex.ToString(),
        _ => message.ToString() ?? string.Empty
    };
}
