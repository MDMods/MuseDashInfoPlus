namespace MDIP.Application.Services.Scoped.Scheduling;

public interface IRefreshScheduler
{
    void OnFixedUpdateTick();
    void OnLateUpdateTick();
}