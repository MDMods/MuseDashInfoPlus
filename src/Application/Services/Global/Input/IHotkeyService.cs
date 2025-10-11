namespace MDIP.Application.Services.Global.Input;

public interface IHotkeyService
{
    bool CheckToggleTriggered();
    void RebindFromConfig();
}