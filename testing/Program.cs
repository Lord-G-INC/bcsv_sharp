using bcsv_sharp;
using binary_stream;

var data = File.ReadAllBytes("HeapSizeExcept.bcsv");

using BinaryStream stream = new(data)
{
    Endian = Endian.Big
};

BCSV bcsv = new(stream);

Console.WriteLine(bcsv.ConvertToCsv(false));