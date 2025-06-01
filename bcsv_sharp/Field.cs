using Dunet;

namespace bcsv_sharp;

[Union]
public partial record FieldType
{
    partial record LONG
    {
        public override ushort Size => 4;
        public override uint Mask => u32.MaxValue;
        public override int Order => 2;
    }
    partial record STRING
    {
        public override ushort Size => 32;
        public override uint Mask => 0;
        public override int Order => 0;
    }
    partial record FLOAT
    {
        public override ushort Size => 4;
        public override uint Mask => 0;
        public override int Order => 1;
    }
    partial record ULONG
    {
        public override ushort Size => 4;
        public override uint Mask => u32.MaxValue;
        public override int Order => 3;
    }
    partial record SHORT
    {
        public override ushort Size => 2;
        public override uint Mask => ushort.MaxValue;
        public override int Order => 4;
    }
    partial record CHAR
    {
        public override ushort Size => 1;
        public override uint Mask => byte.MaxValue;
        public override int Order => 5;
    }
    partial record STRINGOFF
    {
        public override ushort Size => 4;
        public override uint Mask => u32.MaxValue;
        public override int Order => 6;
    }
    partial record NULL
    {
        public override ushort Size => 0;
        public override uint Mask => 0;
        public override int Order => -1;

    }
    public static implicit operator FieldType(u8 dt)
    {
        return dt switch
        {
            0 => new LONG(),
            1 => new STRING(),
            2 => new FLOAT(),
            3 => new ULONG(),
            4 => new SHORT(),
            5 => new CHAR(),
            6 => new STRINGOFF(),
            _ => new NULL(),
        };
    }
    public static implicit operator u8(FieldType ft)
    {
        return ft.Match<u8>((_) => 0, (_) => 1, (_) => 2, (_) => 3, (_) => 4,
            (_) => 5, (_) => 6, (_) => 7);
    }
    public abstract u16 Size { get; }
    public abstract u32 Mask { get; }
    public abstract int Order { get; }
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
        DataType = stream.ReadUnmanaged<u8>();
    }
    public readonly void Write(BinaryStream stream)
    {
        stream.WriteUnmanaged(Hash);
        stream.WriteUnmanaged(Mask);
        stream.WriteUnmanaged(DataOff);
        stream.WriteUnmanaged(Shift);
        stream.WriteUnmanaged<u8>(DataType);
    }
    public readonly string Name(Dictionary<u32, string> map)
    {
        if (map.TryGetValue(Hash, out var name)) return name;
        else return $"0x{Hash:X}";
    }
}