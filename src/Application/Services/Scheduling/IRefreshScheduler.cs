namespace MDIP.Application.Services.Scheduling;

public interface IRefreshScheduler
{
    void OnFixedUpdateTick();
    void OnLateUpdateTick();
    void Reset();
}