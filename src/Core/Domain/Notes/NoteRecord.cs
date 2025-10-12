using MDIP.Core.Domain.Enums;

namespace MDIP.Core.Domain.Notes;

public class NoteRecord(short oid, NoteType type, int doubleId, string longType)
{
    public short Oid { get; } = oid;
    public NoteType Type { get; } = type;
    public int DoubleId { get; } = doubleId;
    public string LongType { get; } = longType;
    public Dictionary<string, List<string>> PatchInfosDic { get; } = new();

    public void AddPatchInfo(string name, string info = "null")
    {
        if (!PatchInfosDic.TryGetValue(name, out var list))
        {
            list = [];
            PatchInfosDic[name] = list;
        }

        list.Add(info);
    }
}