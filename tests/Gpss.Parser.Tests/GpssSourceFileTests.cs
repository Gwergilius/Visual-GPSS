using Gpss.Parser;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class GpssSourceFileTests
{
    [Fact]
    public void Resolve_PathExistsAsGiven_ReturnsItUnchanged()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(tempDir.FullName, "model.gpss");
            File.WriteAllText(path, " GENERATE 10");

            GpssSourceFile.Resolve(path).ShouldBe(path);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData(".gps")]
    [InlineData(".gpss")]
    public void Resolve_PathMissingButFallbackExtensionExists_ReturnsFallbackPath(string extension)
    {
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            var basePath = Path.Combine(tempDir.FullName, "model");
            File.WriteAllText(basePath + extension, " GENERATE 10");

            GpssSourceFile.Resolve(basePath).ShouldBe(basePath + extension);
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void Resolve_GpsTriedBeforeGpss_WhenBothExist()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            var basePath = Path.Combine(tempDir.FullName, "model");
            File.WriteAllText(basePath + ".gps", " GENERATE 10");
            File.WriteAllText(basePath + ".gpss", " GENERATE 20");

            GpssSourceFile.Resolve(basePath).ShouldBe(basePath + ".gps");
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void Resolve_NeitherPathNorFallbacksExist_ThrowsFileNotFoundException()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            var basePath = Path.Combine(tempDir.FullName, "missing");

            Should.Throw<FileNotFoundException>(() => GpssSourceFile.Resolve(basePath));
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }
}
