namespace MDIP.Core.Contracts;

public interface IStatsSaverService
{
    StatsData GetStats(string song);
    void SetStats(string song, StatsData stats);
}