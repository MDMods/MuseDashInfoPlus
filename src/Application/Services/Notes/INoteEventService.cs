namespace MDIP.Application.Services.Notes;

public interface INoteEventService
{
    void HandleSetPlayResult(int idx, byte result, bool isMulStart, bool isMulEnd, bool isLeft);
    void HandleMissCube(int idx, decimal currentTick);
}