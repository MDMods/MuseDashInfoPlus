using System.Collections.Generic;
using System.IO;
using System.Linq;
using Il2CppGameLogic;
using JetBrains.Annotations;
using MDIP.Application.Services.Diagnostic;
using MDIP.Application.Services.Stats;
using MDIP.Domain.Enums;
using MDIP.Domain.Notes;
using MDIP.Utils;

namespace MDIP.Application.Services.Notes;

public class NoteRecordService : INoteRecordService
{
    private Dictionary<int, NoteRecord> _records = new();

    public void Reset()
        => _records = new();

    public void AddRecord(MusicData note, string patchName, string patchInfo)
    {
        var oid = note.objId;
        if (!_records.ContainsKey(oid))
        {
            if (!Helper.IsRegularNote(note.noteData.type))
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

    public void ExportToExcel(string filePath)
    {
        var patchNames = _records.Values.SelectMany(r => r.PatchInfosDic.Keys).Distinct().ToList();
        var lines = new List<string>();
        var header = new[] { "ObjId", "Type", "Double", "LongType" }.Concat(patchNames).ToList();
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
        Logger.Warning($"Excel exported to: {Path.GetFullPath(filePath)}");
    }

    [UsedImplicitly] public required IGameStatsService GameStatsService { get; init; }
    [UsedImplicitly] public required ILogger<NoteRecordService> Logger { get; init; }
}