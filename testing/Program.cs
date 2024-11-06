using bcsv_sharp;
using Binary_Stream;

var data = File.ReadAllBytes("HeapSizeExcept.bcsv");

using BinaryStream stream = new(data)
{
    Endian = Endian.Big
};

BCSV bcsv = new(stream);

Console.WriteLine(bcsv.ConvertToCsv(false));