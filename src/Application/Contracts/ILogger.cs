namespace MDIP.Application.Contracts;

public interface ILogger<T>
{
    void Info(object message);

    void Warning(object message);

    void Error(object message);

    void Fatal(object message);
}