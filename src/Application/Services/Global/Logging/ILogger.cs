namespace MDIP.Application.Services.Global.Logging;

public interface ILogger<T>
{
    void Info(object message);
    void Warn(object message);
    void Error(object message);
    void Fatal(object message);
}