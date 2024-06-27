
namespace bcsv_sharp;

public static class Hash
{
    public static u16 CalcHash(string str)
    {
        u16 ret = 0;
        foreach (var b in Encoding.ASCII.GetBytes(str))
        {
            ret *= 3;
            ret += b;
        }
        return ret;
    }
    public static Dictionary<u32, string> ReadHashes(FileInfo path)
    {
        var text = File.ReadAllText(path.FullName);
        var hashes = new Dictionary<u32, string>();
        foreach (var line in text.Split('\n'))
        {
            if (line.StartsWith('#'))
                continue;
            var hash = CalcHash(line);
            hashes.Add(hash, line);
        }
        return hashes;
    }
}