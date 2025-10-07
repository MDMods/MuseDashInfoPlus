using Il2CppGameLogic;

namespace MDIP.Core.Contracts;

public interface INoteRecordService
{
    void Reset();
    void AddRecord(MusicData note, string patchName, string patchInfo);
    void ExportToExcel(string filePath);
}