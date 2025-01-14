using System.Collections.Generic;
using UnityEngine;

using MuseDashInfoPlus.Modules;

namespace MuseDashInfoPlus.Utils;

public static class Constants
{
    public const int CHART_NAME_SIZE = 38;
    public const int CHART_DIFFICULTY_SIZE = 27;

    public static Vector3 CHART_INFOS_POS = new(720, 470, 0);

    public const int GAME_STATS_SIZE = 36;
    public const int SCORE_STATS_SIZE = 38;
    public const int HIT_STATS_SIZE = 30;

    public static Vector3 GAME_STATS_POS = new(0, -135, 0);
    public static Vector3 SCORE_STATS_POS = new(55, -64, 0);
    public static Vector3 HIT_STATS_POS = new(-866, -460, 0);

    public static int SEPARATIST_SIZE = 34;
    public static string SEPARATIST_COLOR = "#d2d2d2";

    public const string GAP_BEHIND_COLOR = "#9338fb";
    public const string GAP_AHEAD_COLOR = "#fe41f3";

    public static Dictionary<StageType, float> X_BEHIND_SCORE_TEXT = new()
    {
        { StageType.GC, 325 },
        { StageType.Djmax, 260 },
        { StageType.OtherEN, 270 },
        { StageType.OtherCN, 160 },
        { StageType.Unknown, 260 }
    };
    public static Dictionary<StageType, float> Y_BEHIND_SCORE = new()
    {
        { StageType.GC, -55 },
        { StageType.Djmax, -78 },
        { StageType.OtherEN, -64 },
        { StageType.OtherCN, -64 },
        { StageType.Unknown, -64 }
    };
}