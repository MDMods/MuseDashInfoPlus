namespace MDIP.Managers;

public static class TextObjManager
{
    private static ITextObjectService Service => ModServices.GetRequiredService<ITextObjectService>();

    public static GameObject TextLowerLeftObj
    {
        get => Service.TextLowerLeft;
        set => Service.TextLowerLeft = value;
    }

    public static GameObject TextLowerRightObj
    {
        get => Service.TextLowerRight;
        set => Service.TextLowerRight = value;
    }

    public static GameObject TextScoreBelowObj
    {
        get => Service.TextScoreBelow;
        set => Service.TextScoreBelow = value;
    }

    public static GameObject TextScoreRightObj
    {
        get => Service.TextScoreRight;
        set => Service.TextScoreRight = value;
    }

    public static GameObject TextUpperLeftObj
    {
        get => Service.TextUpperLeft;
        set => Service.TextUpperLeft = value;
    }

    public static GameObject TextUpperRightObj
    {
        get => Service.TextUpperRight;
        set => Service.TextUpperRight = value;
    }

    public static void UpdateAllText() => Service.UpdateAllText();
    public static void Reset() => Service.Reset();
}