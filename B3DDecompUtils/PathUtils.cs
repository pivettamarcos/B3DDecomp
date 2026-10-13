namespace B3DDecompUtils;
public static class PathUtils
{
    public static string CleanupPath(this string path)
        => path.Replace("\\", "/");
    
    public static string AppendToPath(this string path, string fsEntry)
    {
        path = path.CleanupPath();
        if (!path.EndsWith("/")) { path += "/"; }
        return path + fsEntry;
    }

    public static bool IsDisasmPath(this string path)
    {
        var directoryName = Path.GetFileName(path.TrimEnd('/', '\\'));
        return directoryName.EndsWith("_disasm", StringComparison.OrdinalIgnoreCase)
               || directoryName.Equals("disasm", StringComparison.OrdinalIgnoreCase);
    }
}
