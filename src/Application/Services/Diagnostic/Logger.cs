namespace MDIP.Application.Services.Diagnostic;

public class Logger<T> : ILogger<T>
{
    private readonly string _typeName = typeof(T).Name;

    public void Info(object message)
        => Melon<MDIPMod>.Logger.Msg(Format(message));

    public void Warning(object message)
        => Melon<MDIPMod>.Logger.Warning(Format(message));

    public void Error(object message)
        => Melon<MDIPMod>.Logger.Error(Format(message));

    public void Fatal(object message)
        => Melon<MDIPMod>.Logger.BigError(Format(message));

    private string Format(object message)
    {
        var content = message switch
        {
            null => "null",
            Exception ex => $"{ex.Message}\n{ex.StackTrace}",
            _ => message.ToString() ?? string.Empty
        };

        return $"[{_typeName}] {content}";
    }
}