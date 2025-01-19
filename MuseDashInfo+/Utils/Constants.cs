using System.Collections.Generic;
using UnityEngine;

using MDIP.Modules;

namespace MDIP.Utils;

public static class Constants
{
    public const int CHART_NAME_SIZE = 38;
    public const int CHART_DIFFICULTY_SIZE = 27;

    public static Vector3 CHART_INFOS_POS = new(720, 470, 0);

    public const int GAME_STATS_SIZE = 36;
    public const int SCORE_STATS_SIZE = 38;
    public const int NOTE_STATS_SIZE = 30;

    public static Vector3 GAME_STATS_POS = new(0, -135, 0);
    public static Vector3 SCORE_STATS_POS = new(55, -64, 0);
    public static Vector3 NOTE_STATS_POS = new(-866, -460, 0);

    public static int SEPARATOR_SIZE = 34;
    public static string SEPARATOR_COLOR = "#d2d2d2";

    public const string GAP_BEHIND_COLOR = "#9338fb";
    public const string GAP_AHEAD_COLOR = "#fe41f3";

    public const string RANK_SSS_COLOR = "#fff000";
    public const string RANK_SS_COLOR = "#ccf0fe";
    public const string RANK_S_COLOR = "#ff0089";
    public const string RANK_A_COLOR = "#ad00ff";
    public const string RANK_B_COLOR = "#00bbff";
    public const string RANK_C_COLOR = "#00ff23";
    public const string RANK_D_COLOR = "#a2a2a2";

    public const string WHITE_COLOR = "#fff";

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