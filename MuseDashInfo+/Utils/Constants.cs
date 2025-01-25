using System.Collections.Generic;
using UnityEngine;

using MDIP.Modules;

namespace MDIP.Utils;

public static class Constants
{
    public const string TEXT_ALL_PERFECT = "AP";
    public const string TEXT_TRUE_PERFECT = "TP";

    public const string TEXT_DIFF_1 = "Easy";
    public const string TEXT_DIFF_2 = "Hard";
    public const string TEXT_DIFF_3 = "Master";
    public const string TEXT_DIFF_4 = "Hidden";
    public const string TEXT_DIFF_5 = "Special";

    public const string COLOR_GAP_BEHIND = "#9338fb";
    public const string COLOR_GAP_AHEAD = "#fe41f3";

    public const string COLOR_RANK_SSS = "#fff000";
    public const string COLOR_RANK_SS = "#ccf0fe";
    public const string COLOR_RANK_S = "#ff0089";
    public const string COLOR_RANK_A = "#ad00ff";
    public const string COLOR_RANK_B = "#00bbff";
    public const string COLOR_RANK_C = "#00ff23";
    public const string COLOR_RANK_D = "#a2a2a2";

    public const string COLOR_WHITE = "#fdfdfa";

    public static Vector3 POS_LOWER_LEFT_TEXT { get; private set; } = new(-866, -460, 0);
    public static Vector3 POS_LOWER_RIGHT_TEXT { get; private set; } = new(700, -460, 0); // TODO
    public static Vector3 POS_SCORE_BELOW_TEXT { get; private set; } = new(0, -135, 0);
    public static Vector3 POS_SCORE_RIGHT_TEXT { get; private set; } = new(55, -64, 0);
    public static Vector3 POS_UPPER_LEFT_TEXT { get; private set; } = new(-866, 470, 0); // TODO
    public static Vector3 POS_UPPER_RIGHT_TEXT { get; private set; } = new(720, 470, 0);

    public static Dictionary<StageType, float> X_OFFSET_SCORE_BELOW_TEXT { get; private set; } = new()
    {
        { StageType.GC, 325 },
        { StageType.Djmax, 260 },
        { StageType.OtherEN, 270 },
        { StageType.OtherCN, 160 },
        { StageType.Unknown, 260 }
    };
    public static Dictionary<StageType, float> Y_OFFSET_SCORE_RIGHT_TEXT { get; private set; } = new()
    {
        { StageType.GC, -55 },
        { StageType.Djmax, -78 },
        { StageType.OtherEN, -64 },
        { StageType.OtherCN, -64 },
        { StageType.Unknown, -64 }
    };
}