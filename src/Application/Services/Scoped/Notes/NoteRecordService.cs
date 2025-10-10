using Il2CppGameLogic;
using JetBrains.Annotations;
using MDIP.Application.DependencyInjection;
using MDIP.Application.Services.Global.Logging;
using MDIP.Application.Services.Scoped.Stats;
using MDIP.Core.Domain.Enums;
using MDIP.Core.Domain.Notes;

namespace MDIP.Application.Services.Scoped.Notes;

public class NoteRecordService : INoteRecordService
{
    private static readonly string[] First = ["ObjId", "Type", "Double", "LongType"];
    private readonly Dictionary<int, NoteRecord> _records = new();

    public void AddRecord(MusicData note, string patchName, string patchInfo)
    {
        var oid = note.objId;
        if (!_records.ContainsKey(oid))
        {
            if (!Enum.IsDefined(typeof(NoteType), note.noteData.type))
                return;

            var longType = "-";
            if (note.isLongPressStart)
                longType = "Start";
            else if (note.isLongPressing)
                longType = "Pressing";
            else if (note.isLongPressEnd)
                longType = "End";

            var doubleId = -1;
            if (note.doubleIdx >= 0)
            {
                try
                {
                    doubleId = GameStatsService.GetMusicDataByIdx(note.doubleIdx).objId;
                }
                catch
                {
                    doubleId = -1;
                }
            }

            _records.Add(oid, new(oid, (NoteType)note.noteData.type, doubleId, longType));
        }

        if (!_records.TryGetValue(oid, out var record))
            return;

        record.AddPatchInfo(patchName, patchInfo);
    }

    public void ExportToCsv(string filePath)
    {
        var patchNames = _records.Values.SelectMany(r => r.PatchInfosDic.Keys).Distinct().ToList();
        var lines = new List<string>();
        var header = First.Concat(patchNames).ToList();
        lines.Add(string.Join(",", header));

        foreach (var kvp in _records.OrderBy(x => x.Key))
        {
            var record = kvp.Value;
            var row = new List<string>
            {
                record.Oid.ToString(),
                record.Type.ToString(),
                record.DoubleId.ToString(),
                record.LongType
            };

            foreach (var name in patchNames)
            {
                if (record.PatchInfosDic.TryGetValue(name, out var infos))
                {
                    row.Add(infos.Count switch
                    {
                        0 => "Empty",
                        1 => infos[0],
                        _ => string.Join(" | ", infos.Select((info, index) => $"({index + 1}) {info}"))
                    });
                }
                else
                    row.Add("-");
            }

            lines.Add(string.Join(",", row));
        }

        File.WriteAllLines(filePath, lines);
        Logger.Info($"Excel exported to: {Path.GetFullPath(filePath)}");
    }

    [UsedImplicitly] [Inject] public IGameStatsService GameStatsService { get; set; }
    [UsedImplicitly] [Inject] public ILogger<NoteRecordService> Logger { get; set; }
}