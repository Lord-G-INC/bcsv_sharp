namespace bcsv_sharp;

public class StringTable : IWrite
{
    Dictionary<string, u32> Table { get; init; } = [];
    u32 Off { get; set; } = 0;

    public u32 this[string key] => Table[key];

    public StringTable Add(string item)
    {
        if (!Table.ContainsKey(item))
        {
            Table.Add(item, Off);
            Off += (u32)(item.Length + 1);
        }
        return this;
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
            if (values[i] is StringOffValue)
            {
                Add(values[i].Data);
                if (Table.TryGetValue(values[i].Data, out uint off))
                    values[i].Data = off;
            }
        }
    }
}