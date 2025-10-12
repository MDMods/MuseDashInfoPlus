using Il2CppGameLogic;

namespace MDIP.Application.Services.Scoped.Notes;

public interface INoteRecordService
{
    void AddRecord(MusicData note, string patchName, string patchInfo);
    void ExportToCsv(string filePath);
}