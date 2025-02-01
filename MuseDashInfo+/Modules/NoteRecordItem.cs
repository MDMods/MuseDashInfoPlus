namespace MDIP.Modules;

public class NoteRecord(
	int id,
	NoteType type,
	int doubleId,
	string longType
)
{
	public int Id { get; private set; } = id;
	public NoteType Type { get; private set; } = type;
	public int DoubleId { get; private set; } = doubleId;
	public string LongType { get; private set; } = longType;

	public Dictionary<string, List<string>> PatchInfosDic { get; private set; } = new();

	public void AddPatchInfo(string name, string info = "null")
	{
		PatchInfosDic ??= new();
		if (!PatchInfosDic.ContainsKey(name))
			PatchInfosDic.Add(name, new());
		PatchInfosDic[name].Add(info);
	}
}