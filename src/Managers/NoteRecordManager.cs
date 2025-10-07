using Il2CppGameLogic;

namespace MDIP.Managers;

public static class NoteRecordManager
{
    private static INoteRecordService Service => ModServices.GetRequiredService<INoteRecordService>();

    public static void Reset() => Service.Reset();
    public static void AddRecord(MusicData note, string patchName, string patchInfo) => Service.AddRecord(note, patchName, patchInfo);
    public static void ExportToExcel(string filePath) => Service.ExportToExcel(filePath);
}