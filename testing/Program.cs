using bcsv_sharp;
using Binary_Stream;

var data = File.ReadAllBytes("AreaObjInfo_2");

using BinaryStream stream = new(data)
{
    Endian = Endian.Big
};

var bcsv = stream.ReadItem<BCSV>();

var hashes = Hash.ReadHashes(new("hashlookup.txt"));

var csv = bcsv.ConvertToCSV(hashes: hashes);

Console.WriteLine(csv);