using Il2CppGameLogic;

namespace MDIP.Application.Services.Notes;

public interface INoteRecordService
{
    void Reset();
    void AddRecord(MusicData note, string patchName, string patchInfo);
    void ExportToExcel(string filePath);
}