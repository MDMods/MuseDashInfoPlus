using MDIP.Domain.Stats;

namespace MDIP.Application.Contracts;

public interface IStatsSaverService
{
    StatsData GetStats(string song);
    void SetStats(string song, StatsData stats);
}