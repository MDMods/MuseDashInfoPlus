using Il2CppAssets.Scripts.UI.Panels;

namespace MDIP.Application.Contracts;

public interface IBattleUIService
{
    void OnGameStart(PnlBattle instance);
    void CheckAndZoom();
    void Reset();
}