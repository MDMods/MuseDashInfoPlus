namespace MDIP.Managers;

public static class NoteRecordManager
{
	public static Dictionary<int, NoteRecord> Records { get; private set; } = new();

	public static void Reset() => Records = new();

	public static void AddRecord(int id, string patchName, string patchInfo)
	{
		if (!Records.ContainsKey(id))
		{
			var note = GameStatsManager.GetMusicDataByIdx(id);
			if (note == null || !Helper.IsRegularNote(note.noteData.type)) return;
			var longType = "-";
			if (note.isLongPressStart) longType = "Start";
			if (note.isLongPressing) longType = "Pressing";
			if (note.isLongPressEnd) longType = "End";
			Records.Add(id, new(id, (NoteType)note.noteData.type, note.doubleIdx, longType));
		}

		Records[id].AddPatchInfo(patchName, patchInfo);
	}

	public static void ExportToExcel(string filePath = @"E:\Desktop\NoteAnalysis.csv")
	{
		var recordDic = Records;
		var patchNames = recordDic.Values
			.SelectMany(r => r.PatchInfosDic.Keys)
			.Distinct()
			.ToList();

		var lines = new List<string>();

		var header = new[] { "ID", "Type", "Double", "LongType", "Result" }
			.Concat(patchNames)
			.ToList();
		lines.Add(string.Join(",", header));

		foreach (var kvp in recordDic.OrderBy(x => x.Key))
		{
			var record = kvp.Value;
			var row = new List<string>
			{
				record.Id.ToString(),
				record.Type.ToString(),
				record.DoubleId.ToString(),
				record.LongType
			};

			foreach (var name in patchNames)
			{
				if (record.PatchInfosDic.TryGetValue(name, out var infos))
				{
					if (infos.Count == 0) row.Add("Empty");
					else if (infos.Count == 1) row.Add(infos[0]);
					else row.Add(string.Join(" | ", infos.Select((info, idx) => $"({idx + 1}) {info}")));
				}
				else row.Add("-");
			}

			lines.Add(string.Join(",", row));
		}

		File.WriteAllLines(filePath, lines);
		Melon<MDIPMod>.Logger.Warning($"Excel exported to: {Path.GetFullPath(filePath)}");
	}
}