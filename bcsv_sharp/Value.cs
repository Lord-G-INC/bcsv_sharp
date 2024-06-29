using System.Numerics;

namespace bcsv_sharp;

public abstract class Value(Field field) : IRead, IWrite
{
    public Field Field { get; init; } = field;

    public abstract dynamic Data { get; internal set; }

    public override string ToString()
    {
        return Data.ToString();
    }

    public virtual string ToString(bool signed) => ToString();

    public abstract void Read(BinaryStream stream);

    public abstract void Write(BinaryStream stream);

    public void ReadVal(BinaryStream stream, int row, Header header)
    {
        stream.SeekTask(row * header.EntrySize + Field.DataOff, SeekOrigin.Current, Read);
    }
}

public class IntValue<T> : Value 
    where T : unmanaged, INumber<T>, IBitwiseOperators<T, T, T>, IShiftOperators<T, int, T>
{
    public override dynamic Data { get; internal set; }

    public IntValue(Field field, T? value = null) : base(field)
    {
        Data = value ?? T.Zero;
        Recalc();
    }

    public void Recalc()
    {
        T data = (T)Data;
        u32 m = Field.DataType.Mask();
        T mask = Unsafe.As<u32, T>(ref m);
        data &= mask;
        data >>= Field.Shift;
        Data = data;
    }

    public override void Read(BinaryStream stream)
    {
        Data = stream.ReadUnmanaged<T>();
        Recalc();
    }

    public override void Write(BinaryStream stream)
    {
        stream.WriteUnmanaged<T>(Data);
    }

    public override string ToString(bool signed)
    {
        if (signed)
        {
            if (Data is ushort sh)
                return Convert.ToInt16(sh).ToString();
            if (Data is byte by)
                return Convert.ToSByte(by).ToString();
        }
        return base.ToString();
    }
}

public class StringOffValue(Field field, u32? value = null) : IntValue<u32>(field, value)
{
    public void ReadStringOff(BinaryStream stream, Header header, Encoding? enc = null)
    {
        var stringoff = header.StringOffset;
        Data = stream.ReadNTStringAt(stringoff + (long)Data, enc);
    }

    public override string ToString(bool signed)
    {
        if (Data is u32 n && signed)
            return Convert.ToInt32(n).ToString();
        return Data.ToString();
    }
}

public class StringValue(Field field) : Value(field)
{
    public override dynamic Data { get; internal set; } = new byte[32];

    public override string ToString()
    {
        return Encoding.UTF8.GetString(Data);
    }

    public override void Read(BinaryStream stream)
    {
        byte[] data = (byte[])Data;
        stream.Read(data);
        Data = data;
    }

    public override void Write(BinaryStream stream)
    {
        byte[] data = (byte[])Data;
        stream.Write(data);
    }
}

public class FloatValue(Field field, f32? value = null) : Value(field)
{
    public override dynamic Data { get; internal set; } = value ?? 0f;

    public override void Read(BinaryStream stream)
    {
        Data = stream.ReadUnmanaged<f32>();
    }

    public override void Write(BinaryStream stream)
    {
        stream.WriteUnmanaged<f32>(Data);
    }
}

public class NullValue(Field field) : Value(field)
{
    public override dynamic Data { get; internal set; } = "NULL";

    public override void Read(BinaryStream stream) { }
    public override void Write(BinaryStream stream) { }
}