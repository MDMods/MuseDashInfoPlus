namespace MDIP.Managers;

public static class StatsSaverManager
{
    private static IStatsSaverService Service => ModServices.GetRequiredService<IStatsSaverService>();

    public static StatsData GetStats(string song) => Service.GetStats(song);
    public static void SetStats(string song, StatsData stats) => Service.SetStats(song, stats);
}