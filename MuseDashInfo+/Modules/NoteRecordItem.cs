using System.Collections.Generic;

namespace MDIP.Modules;

public class NoteRecord
{
	public int Id { get; private set; }
	public NoteType Type { get; private set; }
	public int DoubleId { get; private set; }
	public string LongType { get; private set; }
	public uint Result { get; private set; }

	public Dictionary<string, List<string>> PatchInfosDic { get; private set; } = new();

	public NoteRecord(int id, NoteType type, int doubleId, string longType)
	{
		Id = id;
		Type = type;
		DoubleId = doubleId;
		LongType = longType;
	}

	public void SetNoteResult(uint result) => Result = result;

	public void AddPatchInfo(string name, string info = "null")
	{
		PatchInfosDic ??= new();
		if (!PatchInfosDic.ContainsKey(name))
			PatchInfosDic.Add(name, new());
		PatchInfosDic[name].Add(info);
	}
}