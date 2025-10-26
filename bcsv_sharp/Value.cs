using Dunet;

namespace bcsv_sharp;

[Union]
public partial record Value : IRead, IWrite
{
    partial record Long
    {
        public int Value;
        public override void Read(BinaryStream stream)
        {
            stream.ReadUnmanaged(ref Value);
            Recalc();
        }
        public override void Write(BinaryStream stream)
        {
            CalcWrite();
            stream.WriteUnmanaged(Value);
        }
        public override string ToString() => Value.ToString();
        void Recalc()
        {
            int mask = (int)Field.Mask;
            Value &= mask;
            Value >>>= Field.Shift;
        }
        void CalcWrite()
        {
            Value <<= Field.Shift;
            Value &= (int)Field.Mask;
        }
    };
    partial record String
    {
        public byte[] Value = new byte[32];
        public override void Read(BinaryStream stream)
        {
            stream.ReadExactly(Value);
        }
        public override void Write(BinaryStream stream)
        {
            stream.Write(Value);
        }
        public override string ToString() => Encoding.UTF8.GetString(Value);
    }
    partial record Float
    {
        public f32 Value;
        public override void Read(BinaryStream stream)
        {
            stream.ReadUnmanaged(ref Value);
        }
        public override void Write(BinaryStream stream)
        {
            stream.WriteUnmanaged(Value);
        }
        public override string ToString() => Value.ToString();
    }
    partial record ULong
    {
        public u32 Value;
        public override void Read(BinaryStream stream)
        {
            stream.ReadUnmanaged(ref Value);
            Recalc();
        }
        public override void Write(BinaryStream stream)
        {
            CalcWrite();
            stream.WriteUnmanaged(Value);
        }
        public override string ToString() => Value.ToString();
        void Recalc()
        {
            u32 mask = Field.Mask;
            Value &= mask;
            Value >>>= Field.Shift;
        }
        void CalcWrite()
        {
            Value <<= Field.Shift;
            Value &= Field.Mask;
        }
    }
    partial record Short
    {
        public u16 Value;
        public override void Read(BinaryStream stream)
        {
            stream.ReadUnmanaged(ref Value);
            Recalc();
        }
        public override void Write(BinaryStream stream)
        {
            CalcWrite();
            stream.WriteUnmanaged(Value);
        }
        public override string ToString() => ((short)Value).ToString();
        void Recalc()
        {
            u16 mask = (u16)Field.Mask;
            Value &= mask;
            Value >>>= Field.Shift;
        }
        void CalcWrite()
        {
            Value <<= Field.Shift;
            Value &= (ushort)Field.Mask;
        }
    }
    partial record Char
    {
        public u8 Value;
        public override void Read(BinaryStream stream)
        {
            stream.ReadUnmanaged(ref Value);
            Recalc();
        }
        public override void Write(BinaryStream stream)
        {
            CalcWrite();
            stream.WriteUnmanaged(Value);
        }
        public override string ToString() => ((sbyte)Value).ToString();
        void Recalc()
        {
            u8 mask = (u8)Field.Mask;
            Value &= mask;
            Value >>>= Field.Shift;
        }
        void CalcWrite()
        {
            Value <<= Field.Shift;
            Value &= (u8)Field.Mask;
        }
    }
    partial record StringOff
    {
        public u32 Position;
        public string Value = string.Empty;
        public override void Read(BinaryStream stream)
        {
            stream.ReadUnmanaged(ref Position);
        }
        public override void Write(BinaryStream stream)
        {
            stream.WriteUnmanaged(Position);
        }
        public override string ToString() => Value;
        public void ReadStringOff(BinaryStream stream, Header header, Encoding? enc = null)
        {
            Value = stream.ReadNTStringAt(header.StringOffset + Position, enc);
        }
    }
    partial record Null
    {
        public override void Read(BinaryStream stream)
        {
        }
        public override void Write(BinaryStream stream)
        {
        }
    }
    public abstract void Read(BinaryStream stream);
    public abstract void Write(BinaryStream stream);
    public Field Field { get; protected set; }
    public static Value Create(Field field)
    {
        Value val = field.DataType.Match<Value>((_) => new Long(),
            (_) => new String(), (_) => new Float(), (_) => new ULong(),
            (_) => new Short(), (_) => new Char(), (_) => new StringOff(),
            (_) => new Null());
        val.Field = field;
        return val;
    }
}