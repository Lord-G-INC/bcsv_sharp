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
        for (int i = 0; i < Header.FieldCount; i++)
        {
            Fields.Add(stream.ReadItem<Field>());
            Dictionary.Add(Fields[i], []);
        }
        stream.Seek(Header.EntryDataOff, 0);
        var entrysize = Header.EntryCount * Fields.Count;
        int v = 0, row = 0;
        while (v != entrysize)
        {
            if (v >= entrysize)
                break;
            foreach (var field in Fields)
            {
                Value val = field.DataType.DataType switch
                {
                    FieldType.Type.LONG => new IntValue<int>(field),
                    FieldType.Type.STRING => new StringValue(field),
                    FieldType.Type.FLOAT => new FloatValue(field),
                    FieldType.Type.ULONG => new IntValue<u32>(field),
                    FieldType.Type.SHORT => new IntValue<u16>(field),
                    FieldType.Type.CHAR => new IntValue<u8>(field),
                    FieldType.Type.STRINGOFF => new StringOffValue(field),
                    _ => new NullValue(field)
                };
                val.ReadVal(stream, row, Header);
                if (val is StringOffValue value)
                    value.ReadStringOff(stream, Header);
                Values.Add(val);
                if (Dictionary.TryGetValue(field, out var values))
                    values.Add(val);
                v++;
            }
            row++;
        }
    }
}
