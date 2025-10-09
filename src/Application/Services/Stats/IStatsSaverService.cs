using MDIP.Domain.Stats;

namespace MDIP.Application.Services.Stats;

public interface IStatsSaverService
{
    StatsData GetStats(string song);
    void SetStats(string song, StatsData stats);
}