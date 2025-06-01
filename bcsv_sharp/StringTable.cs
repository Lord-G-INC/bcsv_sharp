namespace bcsv_sharp;

public class StringTable : IWrite
{
    Dictionary<string, u32> Table { get; init; } = [];
    u32 Off { get; set; } = 0;
    public u32 this[string key] => Table[key];
    public u32 Add(string name)
    {
        if (!Table.ContainsKey(name))
        {
            Table.Add(name, Off);
            Off += (u32)(name.Length + 1);
        }
        return this[name];
    }
    public void Write(BinaryStream stream)
    {
        foreach (var str in Table.Keys)
            stream.WriteNTString(str);
    }
    public void Update_Offs(List<Value> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            Value val = values[i];
            val.MatchStringOff((x) => { x.Position = Add(x.Value); }, () => { });
            values[i] = val;
        }
    }
}