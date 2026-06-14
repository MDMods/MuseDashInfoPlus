using System.Text;
using Il2CppGameLogic;
using MDIP.Core.Domain.Enums;
using MDIP.Core.Domain.Notes;
using MDIP.Globals;

namespace MDIP.Battle;

// Optional per-note diagnostics, exported to a desktop CSV when the advanced debug option is on.
// Owned by a BattleSession; reads note data through its sibling GameStats.
public class NoteRecords(GameStats stats)
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
                    doubleId = stats.GetMusicDataByIdx(note.doubleIdx).objId;
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
        var patchNames = _records.Values
            .SelectMany(r => r.PatchInfosDic.Keys)
            .Distinct()
            .ToList();

        var lines = new List<string>();

        var header = First.Concat(patchNames).ToList();
        lines.Add(string.Join(",", header.Select(ToCsvCell)));

        foreach (var kvp in _records.OrderBy(x => x.Key))
        {
            var record = kvp.Value;

            var row = new List<string>
            {
                ToCsvCell(record.Oid.ToString()),
                ToCsvCell(record.Type.ToString()),
                ToCsvCell(record.DoubleId.ToString()),
                ToCsvCell(record.LongType)
            };

            foreach (var name in patchNames)
            {
                if (record.PatchInfosDic.TryGetValue(name, out var infos))
                {
                    var cell = infos.Count switch
                    {
                        0 => "Empty",
                        1 => infos[0],
                        _ => string.Join(" | ", infos.Select((info, index) => $"({index + 1}) {info}"))
                    };

                    row.Add(ToCsvCell(cell));
                }
                else
                {
                    row.Add(ToCsvCell("-"));
                }
            }

            lines.Add(string.Join(",", row));
        }

        WriteAllLinesUtf8WithBom(filePath, lines);
        Log.Info($"Excel exported to: {Path.GetFullPath(filePath)}");
    }

    private static string ToCsvCell(string value)
    {
        value ??= string.Empty;

        var requiresQuotes =
            value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r');

        if (!requiresQuotes)
            return value;

        var escaped = value.Replace("\"", "\"\"");
        return $"\"{escaped}\"";
    }

    private static void WriteAllLinesUtf8WithBom(string path, IEnumerable<string> lines)
    {
        var content = string.Join(Environment.NewLine, lines);
        var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true); // UTF8 with BOM
        File.WriteAllText(path, content, utf8Bom);
    }
}
