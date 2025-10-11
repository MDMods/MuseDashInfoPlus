using MDIP.Core.Domain.Stats;

namespace MDIP.Application.Services.Global.Stats;

public interface IStatsSaverService
{
    StatsData GetStats(string song);
    void SetStats(string song, StatsData stats);
}