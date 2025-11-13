namespace bcsv_sharp;

public class BCSV : IRead, IWrite, ILoadable<BCSV>
{
    public Header Header;
    public List<Field> Fields = [];
    public Dictionary<Field, List<Value>> Values = [];
    public void Read(BinaryStream stream)
    {
        stream.ReadItem(ref Header);
        Fields.Capacity = (int)Header.FieldCount;
        Values.EnsureCapacity((int)Header.FieldCount);
        for (int i = 0; i < Fields.Capacity; i++)
        {
            var field = stream.ReadItem<Field>();
            Fields.Add(field);
            var values = new List<Value>((int)Header.EntryCount);
            Values.Add(field, values);
        }
        for (int i = 0; i < Header.EntryCount; i++)
        {
            stream.Seek(Header.EntryDataOff + (Header.EntrySize * i), 0);
            foreach (var f in Fields.OrderBy(x => x.DataOff))
            {
                using Seek<BinaryStream> seek = new(stream, f.DataOff, SeekOrigin.Current);
                Value val = Value.Create(f);
                val.Read(stream);
                val.MatchStringOff((x) => x.ReadStringOff(stream, Header), () => { });
                Values[f].Add(val);
            }
        }
    }
    public Field[] SortedFields() => [.. Fields.OrderBy(x => x.DataType.Order)];
    protected void Update_Data()
    {
        Header.FieldCount = (uint)Fields.Count;
        var sorted = SortedFields();
        u16 doff = 0;
        foreach (var f in sorted)
        {
            var index = Fields.FindIndex(x => x.Hash == f.Hash);
            var og = Fields[index];
            var vals = Values[f];
            if (Header.EntryCount != vals.Count)
                Header.EntryCount = (uint)vals.Count;
            Values.Remove(og);
            og.DataOff = doff;
            doff += og.DataType.Size;
            Fields[index] = og;
            Values.Add(og, vals);
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
        foreach (var vals in Values.Values)
            table.Update_Offs(vals);
        var sorted = SortedFields();
        for (int i = 0; i < Header.EntryCount; i++)
        {
            foreach (var f in sorted)
            {
                Value val = Values[f][i];
                stream.WriteItem(val);
            }
        }
        stream.Seek(Header.StringOffset, 0);
        stream.WriteItem(table);
        var padded = (stream.Position % 32) switch
        {
            0 => stream.Position,
            var x => stream.Position + (32 - x)
        };
        while (stream.Position != padded)
            stream.WriteByte(0x40);
    }
    public string ConvertToCSV(char delim = ',', Dictionary<u32, string>? hashes = null)
    {
        hashes ??= [];
        StringBuilder builder = new();
        for (int i = 0; i < Fields.Count; i++)
        {
            bool last = i == Fields.Count - 1;
            var term = last switch { true => '\n', false => delim };
            builder.Append($"{Fields[i].Name(hashes)}:{(u8)Fields[i].DataType}{term}");
        }
        for (int i = 0; i < Header.EntryCount; i++)
        {
            for (int f = 0; f < Fields.Count; f++)
            {
                bool last = f == Fields.Count - 1;
                var term = last switch { true => '\n', false => delim };
                builder.Append(Values[Fields[f]][i].ToString()).Append(term);
            }
        }
        return builder.ToString();
    }

    public static BCSV LoadFrom(BinaryStream stream, Endian? endian = null, Encoding? enc = null)
    {
        stream.Endian = endian ?? stream.Endian;
        stream.Encoding = enc ?? stream.Encoding;
        return stream.ReadItem<BCSV>();
    }

    public Field CreateField(string name, FieldType datatype)
    {
        Field field = new() { 
            Hash = Hash.CalcHash(name), 
            DataType = datatype,
            Mask = datatype.Mask,
        };
        Fields.Add(field);
        Values.Add(field, new((int)Header.EntryCount));
        return field;
    }

    public int TotalSize()
    {
        /// Header + (Field * FieldCount)
        int total = 16 + (12 * Fields.Count);
        // Total size of values (not stringtable)
        total += (int)Header.EntryCount * Fields.Sum(x => x.DataType.Size);
        // String table calculation
        StringTable table = new();
        for (int i = 0; i < Fields.Count; i++)
            table.Update_Offs(Values[Fields[i]]);
        total += table.TotalSize();
        // Algin to 32
        total = (total % 32) switch
        {
            0 => total,
            var r => total + (32 - r)
        };
        return total;
    }
}