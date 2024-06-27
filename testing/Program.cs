using bcsv_sharp;
using binary_stream;

var bytes = File.ReadAllBytes("HeapSizeExcept.bcsv");

using var stream = new BinaryStream(bytes)
{
    Endian = Endian.Big
};

BCSV bcsv = stream.ReadItem<BCSV>();

Console.WriteLine(bcsv);