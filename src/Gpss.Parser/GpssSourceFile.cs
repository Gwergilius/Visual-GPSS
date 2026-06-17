namespace Gpss.Parser;

/// <summary>
/// Resolves a GPSS source file path on disk, trying well-known extensions when the literal path
/// doesn't exist.
/// </summary>
/// <remarks>
/// Mirrors the legacy GPSS World/GPSS PC convention of defaulting to a <c>.gps</c> (the original,
/// DOS-era extension) or <c>.gpss</c> source file. Used both for the initial source file and for
/// resolving an <c>INCLUDE</c> target, so the same fallback applies in both places.
/// </remarks>
public static class GpssSourceFile
{
    /// <summary>Extensions tried, in order, when a path doesn't exist as given.</summary>
    public static readonly IReadOnlyList<string> FallbackExtensions = [".gps", ".gpss"];

    /// <summary>
    /// Returns <paramref name="path"/> unchanged if it exists; otherwise the first of
    /// <paramref name="path"/> with each of <see cref="FallbackExtensions"/> appended, in order,
    /// that exists.
    /// </summary>
    /// <param name="path">The requested file path, with or without an extension.</param>
    /// <returns>An existing file path.</returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when neither <paramref name="path"/> nor any fallback-extension variant exists.
    /// </exception>
    public static string Resolve(string path)
    {
        if (File.Exists(path)) return path;

        foreach (var extension in FallbackExtensions)
        {
            var candidate = path + extension;
            if (File.Exists(candidate)) return candidate;
        }

        var tried = string.Join(", ",
            new[] { path }.Concat(FallbackExtensions.Select(extension => path + extension)).Select(p => $"'{p}'"));
        throw new FileNotFoundException($"GPSS source file not found. Tried: {tried}.", path);
    }
}
