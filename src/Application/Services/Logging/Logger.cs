namespace MDIP.Application.Services.Logging;

public class Logger<T> : ILogger<T>
{
    public void Info(object message)
        => Melon<MDIPMod>.Logger.Msg(Format(message));

    public void Warn(object message)
        => Melon<MDIPMod>.Logger.Warning(Format(message));

    public void Error(object message)
        => Melon<MDIPMod>.Logger.Error(Format(message));

    public void Fatal(object message)
        => Melon<MDIPMod>.Logger.BigError(Format(message));

    private static string Format(object message)
    {
        var content = message switch
        {
            null => "null",
            Exception ex => ex.ToString(),
            _ => message.ToString() ?? string.Empty
        };

        return $"{(typeof(T).Name == nameof(MDIPMod) ? string.Empty : $"[{typeof(T).Name}] ")}{content}";
    }
}