using Gpss.Parser;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class GpssReaderTests
{
    [Fact]
    public void ReadLine_SkipsBlankAndWhitespaceLines()
    {
        const string source = """
            GENERATE 10


            TERMINATE 1
            """;

        using var reader = new GpssReader(new StringReader(source));

        reader.ReadLine().ShouldBe("GENERATE 10");
        reader.ReadLine().ShouldBe("TERMINATE 1");
        reader.ReadLine().ShouldBeNull();
    }

    [Fact]
    public void ReadLine_SkipsFullLineComment()
    {
        const string source = """
            * full-line comment
            GENERATE 10
            """;

        using var reader = new GpssReader(new StringReader(source));

        reader.ReadLine().ShouldBe("GENERATE 10");
        reader.ReadLine().ShouldBeNull();
    }

    [Fact]
    public void ReadLine_StripsInlineComment()
    {
        using var reader = new GpssReader(new StringReader("GENERATE 10  ; every 10 units"));

        reader.ReadLine().ShouldBe("GENERATE 10");
    }

    [Fact]
    public void ReadLine_TracksOriginalSourceLineNumber()
    {
        const string source = """
            * comment

            GENERATE 10
            TERMINATE 1
            """;

        using var reader = new GpssReader(new StringReader(source));

        reader.ReadLine().ShouldBe("GENERATE 10");
        reader.LineNumber.ShouldBe(3);

        reader.ReadLine().ShouldBe("TERMINATE 1");
        reader.LineNumber.ShouldBe(4);
    }

    [Fact]
    public void ReadLine_EmptySource_ReturnsNull()
    {
        using var reader = new GpssReader(new StringReader(string.Empty));

        reader.ReadLine().ShouldBeNull();
        reader.LineNumber.ShouldBe(0);
    }
}
