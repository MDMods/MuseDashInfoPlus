using Il2CppGameLogic;

namespace MDIP.Managers;

public static class NoteRecordManager
{
    private static Dictionary<int, NoteRecord> Records { get; set; } = new();

    public static void Reset() => Records = new();

    public static void AddRecord(MusicData note, string patchName, string patchInfo)
    {
        var oid = note.objId;
        if (!Records.ContainsKey(oid))
        {
            if (!Helper.IsRegularNote(note.noteData.type)) return;
            var longType = "-";
            if (note.isLongPressStart) longType = "Start";
            if (note.isLongPressing) longType = "Pressing";
            if (note.isLongPressEnd) longType = "End";
            Records.Add(oid, new(oid, (NoteType)note.noteData.type, GameStatsManager.GetMusicDataByIdx(note.doubleIdx).objId, longType));
        }

        Records[oid].AddPatchInfo(patchName, patchInfo);
    }

    public static void ExportToExcel(string filePath)
    {
        var recordDic = Records;
        var patchNames = recordDic.Values
            .SelectMany(r => r.PatchInfosDic.Keys)
            .Distinct()
            .ToList();

        var lines = new List<string>();

        var header = new[] { "ObjId", "Type", "Double", "LongType" }
            .Concat(patchNames)
            .ToList();
        lines.Add(string.Join(",", header));

        foreach (var kvp in recordDic.OrderBy(x => x.Key))
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
                            row.Add(string.Join(" | ", infos.Select((info, oid) => $"({oid + 1}) {info}")));
                            break;
                    }
                }
                else row.Add("-");
            }

            lines.Add(string.Join(",", row));
        }

        File.WriteAllLines(filePath, lines);
        Melon<MDIPMod>.Logger.Warning($"Excel exported to: {Path.GetFullPath(filePath)}");
    }
}