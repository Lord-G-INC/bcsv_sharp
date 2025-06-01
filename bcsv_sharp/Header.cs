namespace bcsv_sharp;

public record struct Header : IRead, IWrite
{
    public u32 EntryCount, FieldCount, EntryDataOff, EntrySize;

    public readonly long StringOffset => EntryDataOff + EntryCount * EntrySize;

    public void Read(BinaryStream stream)
    {
        stream.ReadUnmanaged(ref EntryCount);
        stream.ReadUnmanaged(ref FieldCount);
        stream.ReadUnmanaged(ref EntryDataOff);
        stream.ReadUnmanaged(ref EntrySize);
    }

    public readonly void Write(BinaryStream stream)
    {
        stream.WriteUnmanaged(EntryCount);
        stream.WriteUnmanaged(FieldCount);
        stream.WriteUnmanaged(EntryDataOff);
        stream.WriteUnmanaged(EntrySize);
    }

    
}