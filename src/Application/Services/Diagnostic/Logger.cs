using MDIP.Application.Contracts;

namespace MDIP.Application.Services.Diagnostic;

public class Logger<T> : ILogger<T>
{
    private readonly string _typeName = typeof(T).Name;

    public void Info(object message)
        => Melon<MDIPMod>.Logger.Msg(Format("INFO", message));

    public void Warning(object message)
        => Melon<MDIPMod>.Logger.Warning(Format("WARN", message));

    public void Error(object message)
        => Melon<MDIPMod>.Logger.Error(Format("ERROR", message));

    public void Fatal(object message)
        => Melon<MDIPMod>.Logger.BigError(Format("FATAL", message));

    private string Format(string level, object message)
    {
        var content = message switch
        {
            null => "null",
            Exception ex => $"{ex.Message}\n{ex.StackTrace}",
            _ => message.ToString() ?? string.Empty
        };

        return $"[{_typeName}] [{level}] {content}";
    }
}