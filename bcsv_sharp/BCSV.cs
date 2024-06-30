using System.Security.Cryptography;

namespace bcsv_sharp;

public class BCSV : IRead
{
    public Header Header;
    public List<Field> Fields { get; init; } = [];
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

    public Field[] Sorted_Fields()
    {
        return [.. Fields.OrderBy((x) => x.DataType.Order())];
    }

    protected void Update_Data()
    {
        var sorted = Sorted_Fields();
        u16 doff = 0;
        foreach (var f in sorted)
        {
            var index = Fields.FindIndex((x) => x.Hash == f.Hash);
            if (Header.EntryCount is 0)
                Header.EntryCount = (uint)Values.Count;
            var og = Fields[index];
            var vals = Dictionary[og];
            Dictionary.Remove(og);
            og.DataOff = doff;
            doff += og.DataType.Size();
            Fields[index] = og;
            Dictionary.Add(og, vals);
        }
        Header.EntrySize = doff;
        Header.EntryDataOff = 16 + (12 * Header.FieldCount);
    }

    public void Write(BinaryStream stream)
    {
        Update_Data();
        stream.WriteItem(Header);
        foreach (var field in Fields)
            stream.WriteItem(field);
        StringTable table = new();
        table.Update_Offs(Values);
        var v = 0;
        var dict = Dictionary;
        var sorted = Sorted_Fields();
        while (v != Values.Count)
        {
            if (v >= Values.Count)
                break;
            foreach (var field in sorted)
            {
                var val = dict[field][0];
                stream.WriteItem(val);
                dict[field].RemoveAt(0);
                v++;
            }
        }
        stream.Seek(Header.StringOffset, 0);
        stream.WriteItem(table);
        var padded = stream.Position + ((stream.Position + 31 & ~31) - stream.Position);
        while (stream.Position != padded)
            stream.WriteByte(0x40);
    }

    public byte[] ToBytes(Endian endian, Encoding? enc = null)
    {
        using var stream = new BinaryStream()
        {
            Endian = endian,
            Encoding = enc ?? Encoding.UTF8
        };
        Write(stream);
        return stream.ToArray();
    }

    public Value this[int index] { get => Values[index]; set => Values[index] = value; }

    public Value[] this[Field field] => [.. Dictionary[field]];

    public BCSV AddField(Field field, List<Value>? values = null)
    {
        Dictionary[field] = values ?? [];
        return this;
    }
}
