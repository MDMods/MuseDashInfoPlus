// src/Application/Services/Scoped/UI/IBattleUIService.cs
using Il2CppAssets.Scripts.UI.Panels;

namespace MDIP.Application.Services.Scoped.UI;

public interface IBattleUIService : IDisposable
{
    void OnGameStart(PnlBattle instance);
    void CheckAndZoom();
    void QueueApplyConfigChanges();
    void ApplyPendingConfigChanges();
    bool NativeZoomInCompleted { get; }
    bool DesiredUiVisible { get; }
    void SetDesiredUiVisible(bool visible);
}