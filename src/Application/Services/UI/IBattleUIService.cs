using Il2CppAssets.Scripts.UI.Panels;

namespace MDIP.Application.Services.UI;

public interface IBattleUIService
{
    void OnGameStart(PnlBattle instance);
    void CheckAndZoom();
    void Reset();
}