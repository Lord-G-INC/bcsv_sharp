namespace bcsv_sharp;

public class BCSV : IRead
{
    public Header Header;
    public List<Field> Fields = [];
    List<Value> Values { get; init; } = [];
    Dictionary<Field, List<Value>> Dictionary { get; init; } = [];

    public void Read(BinaryStream stream)
    {
        stream.ReadItem(ref Header);
        Fields.Capacity = (int)Header.FieldCount;
        Values.Capacity = (int)(Header.EntryCount * Header.FieldCount);
        Dictionary.EnsureCapacity(Fields.Capacity);
        for (int i = 0; i < Header.FieldCount; i++)
        {
            Fields.Add(stream.ReadItem<Field>());
            Dictionary.Add(Fields[i], new((int)Header.EntryCount));
        }
        stream.Seek(Header.EntryDataOff, 0);
        int row = 0;
        while (Values.Count != Values.Capacity)
        {
            foreach (var field in Fields)
            {
                Value val = field.DataType.DataType switch
                {
                    FieldType.Type.LONG => new IntValue<int>(field),
                    FieldType.Type.STRING => new StringValue(field),
                    FieldType.Type.FLOAT => new FloatValue(field),
                    FieldType.Type.ULONG => new IntValue<uint>(field),
                    FieldType.Type.SHORT => new IntValue<u16>(field),
                    FieldType.Type.CHAR => new IntValue<u8>(field),
                    FieldType.Type.STRINGOFF => new StringOffValue(field),
                    _ => new NullValue(field)
                };
                val.ReadVal(stream, row, Header);
                if (val is StringOffValue v)
                    v.ReadStringOff(stream, Header);
                Values.Add(val);
                Dictionary[field].Add(val);
            }
            row++;
        }
    }

    public string ConvertToCsv(bool signed, char delim = ',', Dictionary<u32, string>? hashes = null)
    {
        hashes ??= [];
        StringBuilder builder = new();
        for (int i = 0; i < Fields.Count; i++)
        {
            bool last = i == Fields.Count - 1;
            var term = last switch { true => '\n', false => delim };
            builder.Append($"{Fields[i].Name(hashes)}:{Fields[i].DataType.DataType}{term}");
        }
        var v = 0;
        while (v < Values.Count)
        {
            for (int i = 0; i < Fields.Count; i++)
            {
                bool last = i == Fields.Count - 1;
                var term = last switch { true => '\n', false => delim };
                builder.Append(Values[v].ToString(signed)).Append(term);
                v++;
            }
        }
        return builder.ToString();
    }
}
