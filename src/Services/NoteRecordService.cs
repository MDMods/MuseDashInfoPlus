using Il2CppGameLogic;

namespace MDIP.Services;

public class NoteRecordService : INoteRecordService
{
    private readonly MelonLogger.Instance _logger;
    private Dictionary<int, NoteRecord> _records;

    public NoteRecordService(MelonLogger.Instance logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _records = new();
    }

    public void Reset()
    {
        _records = new();
    }

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
                    doubleId = GameStatsManager.GetMusicDataByIdx(note.doubleIdx).objId;
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
        var patchNames = _records.Values
            .SelectMany(r => r.PatchInfosDic.Keys)
            .Distinct()
            .ToList();

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
                    switch (infos.Count)
                    {
                        case 0:
                            row.Add("Empty");
                            break;
                        case 1:
                            row.Add(infos[0]);
                            break;
                        default:
                            row.Add(string.Join(" | ", infos.Select((info, index) => $"({index + 1}) {info}")));
                            break;
                    }
                }
                else
                    row.Add("-");
            }

            lines.Add(string.Join(",", row));
        }

        File.WriteAllLines(filePath, lines);
        _logger.Warning($"Excel exported to: {Path.GetFullPath(filePath)}");
    }
}