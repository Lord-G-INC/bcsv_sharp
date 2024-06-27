namespace bcsv_sharp;

public record struct FieldType
{
    public enum Type : u8
    {
        LONG,
        STRING,
        FLOAT,
        ULONG,
        SHORT,
        CHAR,
        STRINGOFF,
        NULL
    }

    public Type DataType;

    public FieldType(u8 dt) { DataType = (Type)dt; }

    public static implicit operator FieldType(u8 dt) => new(dt);

    public static implicit operator u8(FieldType type) => (u8)type.DataType;

    public static implicit operator Type(FieldType type) => type.DataType;

    public static implicit operator FieldType(Type type) => new((u8)type);

    public readonly u16 Size()
    {
        return DataType switch
        {
            Type.LONG or Type.ULONG or Type.FLOAT or Type.STRINGOFF => 4,
            Type.SHORT => 2,
            Type.CHAR => 1,
            Type.STRING => 32,
            _ => 0,
        };
    }

    public readonly u32 Mask()
    {
        return DataType switch
        {
            Type.LONG or Type.ULONG or Type.STRINGOFF => u32.MaxValue,
            Type.SHORT => u16.MaxValue,
            Type.CHAR => u8.MaxValue,
            _ => 0
        };
    }

    public readonly int Order()
    {
        return DataType switch
        {
            Type.LONG => 2,
            Type.STRING => 0,
            Type.FLOAT => 1,
            Type.ULONG => 3,
            Type.SHORT => 4,
            Type.CHAR => 5,
            Type.STRINGOFF => 6,
            _ => -1
        };
    }
}

public record struct Field : IRead, IWrite
{
    public u32 Hash, Mask;
    public u16 DataOff;
    public u8 Shift;
    public FieldType DataType;

    public void Read(BinaryStream stream)
    {
        stream.ReadUnmanaged(ref Hash);
        stream.ReadUnmanaged(ref Mask);
        stream.ReadUnmanaged(ref DataOff);
        stream.ReadUnmanaged(ref Shift);
        stream.ReadUnmanaged(ref DataType);
    }

    public readonly void Write(BinaryStream stream)
    {
        stream.WriteUnmanaged(Hash);
        stream.WriteUnmanaged(Mask);
        stream.WriteUnmanaged(DataOff);
        stream.WriteUnmanaged(Shift);
        stream.WriteUnmanaged(DataType);
    }

    public readonly string Name(Dictionary<u32, string> map)
    {
        if (map.TryGetValue(Hash, out var name)) return name;
        else return $"0x{Hash:X}";
    }
}