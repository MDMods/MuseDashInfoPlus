using Il2CppAssets.Scripts.UI.Panels;

namespace MDIP.Application.Services.Scoped.UI;

public interface IBattleUIService
{
    void OnGameStart(PnlBattle instance);
    void CheckAndZoom();
}