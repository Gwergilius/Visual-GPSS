using Gpss.Parser;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class GpssReaderTests
{
    [Fact]
    public void Read_SkipsBlankAndWhitespaceLines()
    {
        // Indented (no label): the verb is never in column 1.
        const string source = " GENERATE 10\n\n\n TERMINATE 1";

        using var reader = new GpssReader(new StringReader(source));

        reader.Read()!.Verb.ShouldBe("GENERATE");
        reader.Read()!.Verb.ShouldBe("TERMINATE");
        reader.Read().ShouldBeNull();
    }

    [Fact]
    public void Read_SkipsFullLineComment()
    {
        const string source = "* full-line comment\n GENERATE 10";

        using var reader = new GpssReader(new StringReader(source));

        reader.Read()!.Verb.ShouldBe("GENERATE");
        reader.Read().ShouldBeNull();
    }

    [Fact]
    public void Read_StripsInlineCommentFromOperands()
    {
        using var reader = new GpssReader(new StringReader(" GENERATE 10  ; every 10 units"));

        var statement = reader.Read();

        statement!.Verb.ShouldBe("GENERATE");
        statement.Operands.ShouldBe(["10"]);
    }

    [Fact]
    public void Read_CapturesInlineCommentForCurrentStatement()
    {
        using var reader = new GpssReader(new StringReader(" GENERATE 10  ; every 10 units"));

        reader.Read()!.Comment.ShouldBe("every 10 units");
    }

    [Fact]
    public void Read_NoInlineComment_CommentIsNull()
    {
        using var reader = new GpssReader(new StringReader(" GENERATE 10"));

        reader.Read()!.Comment.ShouldBeNull();
    }

    [Theory]
    [InlineData("GEN1 GENERATE 10", "GEN1", "GENERATE")]
    [InlineData(" GENERATE 10", null, "GENERATE")]
    public void Read_DistinguishesLabelFromVerb(string line, string? expectedLabel, string expectedVerb)
    {
        // Purely positional: a flush-left first word (no leading whitespace) is a label; an
        // indented line has no label and starts directly with the verb.
        using var reader = new GpssReader(new StringReader(line));

        var statement = reader.Read();

        statement!.Label.ShouldBe(expectedLabel);
        statement.Verb.ShouldBe(expectedVerb);
    }

    [Fact]
    public void Read_FlushLeftUnlabeledStatement_IsReadAsLabelPlusMalformedVerb()
    {
        // The verb is never in column 1, so a flush-left, unindented line is always read as
        // having a label. An unlabeled statement must therefore be indented; writing one flush
        // left (e.g. "TERMINATE 1" with nothing before it) is malformed input by design - it is
        // read as label "TERMINATE" with verb "1", which the parser later rejects as an unknown
        // block name rather than this reader trying to guess the "intended" meaning.
        using var reader = new GpssReader(new StringReader("TERMINATE 1"));

        var statement = reader.Read();

        statement!.Label.ShouldBe("TERMINATE");
        statement.Verb.ShouldBe("1");
    }

    [Theory]
    [InlineData(" ADVANCE 10,3", "10", "3")]
    [InlineData(" ADVANCE 10 , 3", "10", "3")]
    [InlineData(" ADVANCE  10  ,  3  ", "10", "3")]
    public void Read_OperandsWithSpacesAroundCommas_AreSplitCorrectly(string line, string a, string b)
    {
        using var reader = new GpssReader(new StringReader(line));

        reader.Read()!.Operands.ShouldBe([a, b]);
    }

    [Fact]
    public void Read_QuotedOperand_KeepsEmbeddedCommaAndWhitespaceAsOneOperand()
    {
        // Quoting is a generic operand feature, not something special-cased for INCLUDE: any
        // block's operand can be quoted to protect a comma or whitespace from being split on.
        using var reader = new GpssReader(new StringReader(" QUEUE \"Barber, Senior\""));

        var statement = reader.Read();

        statement!.Verb.ShouldBe("QUEUE");
        statement.Operands.ShouldBe(["Barber, Senior"]);
    }

    [Fact]
    public void Read_UnterminatedQuoteInOperand_Throws()
    {
        using var reader = new GpssReader(new StringReader(" QUEUE \"Barber"));

        Should.Throw<FormatException>(() => reader.Read());
    }

    [Fact]
    public void Read_OperandMixingBareAndQuotedRuns_ConcatenatesThemIntoOneOperand()
    {
        // operand: (char | string)* - bare and quoted runs may be freely interleaved within a
        // single operand; the quotes are always delimiters, never literal data, regardless of
        // where they occur, and the comma inside the quoted run does not split the operand.
        using var reader = new GpssReader(new StringReader(" QUEUE ab\"cd,ef\"gh"));

        reader.Read()!.Operands.ShouldBe(["abcd,efgh"]);
    }

    [Fact]
    public void Read_OperandIsStringExpression_QuotingProtectsOnlyTheLiteralArgument()
    {
        // The motivating case for mixing: an operand can itself be a string expression, where
        // quoting protects just the string literal argument (here, its embedded comma) while the
        // surrounding call syntax is bare characters in the same operand.
        using var reader = new GpssReader(new StringReader(" QUEUE ToUpper(\"lower, case sentence\")"));

        reader.Read()!.Operands.ShouldBe(["ToUpper(lower, case sentence)"]);
    }

    [Fact]
    public void Read_TracksOriginalSourceLineNumber()
    {
        const string source = "* comment\n\n GENERATE 10\n TERMINATE 1";

        using var reader = new GpssReader(new StringReader(source));

        reader.Read()!.LineNumber.ShouldBe(3);
        reader.Read()!.LineNumber.ShouldBe(4);
    }

    [Fact]
    public void Read_DefaultFileName_IsEmptyString()
    {
        using var reader = new GpssReader(new StringReader(" GENERATE 10"));

        reader.Read()!.FileName.ShouldBe("");
    }

    [Fact]
    public void Read_ExplicitFileName_IsReflectedOnStatementAndAssignedFileNumberOne()
    {
        using var reader = new GpssReader(new StringReader(" GENERATE 10"), "main.gpss");

        var statement = reader.Read();

        statement!.FileName.ShouldBe("main.gpss");
        statement.FileNumber.ShouldBe(1);
    }

    [Fact]
    public void Read_EmptySource_ReturnsNull()
    {
        using var reader = new GpssReader(new StringReader(string.Empty));

        reader.Read().ShouldBeNull();
    }

    [Fact]
    public void Read_Include_SwitchesToIncludedFileThenResumesParentAfterIncludeLine()
    {
        const string parentSource = " GENERATE 10\n INCLUDE child.gpss\n TERMINATE 1";
        const string childSource = " QUEUE Barber";

        using var reader = new GpssReader(new StringReader(parentSource), "parent.gpss",
            name => name == "child.gpss" ? new StringReader(childSource) : throw new FileNotFoundException(name));

        var first = reader.Read();
        first!.Verb.ShouldBe("GENERATE");
        first.FileName.ShouldBe("parent.gpss");
        first.FileNumber.ShouldBe(1);
        first.LineNumber.ShouldBe(1);

        var included = reader.Read();
        included!.Verb.ShouldBe("QUEUE");
        included.FileName.ShouldBe("child.gpss");
        included.FileNumber.ShouldBe(2);
        included.LineNumber.ShouldBe(1);

        var resumed = reader.Read();
        resumed!.Verb.ShouldBe("TERMINATE");
        resumed.FileName.ShouldBe("parent.gpss");
        resumed.FileNumber.ShouldBe(1);
        resumed.LineNumber.ShouldBe(3);

        reader.Read().ShouldBeNull();
    }

    [Fact]
    public void Read_Include_ResolvesRelativeNameAgainstParentDirectory()
    {
        const string parentSource = " INCLUDE child.gpss";
        var requestedNames = new List<string>();

        using var reader = new GpssReader(new StringReader(parentSource), Path.Combine("sub", "parent.gpss"),
            name =>
            {
                requestedNames.Add(name);
                return new StringReader(" TERMINATE 1");
            });

        reader.Read();

        requestedNames.ShouldBe([Path.Combine("sub", "child.gpss")]);
    }

    [Theory]
    [InlineData(" INCLUDE \"my files\\child.gpss\"", "my files\\child.gpss")]
    [InlineData(" INCLUDE 'my files\\child.gpss'", "my files\\child.gpss")]
    public void Read_Include_QuotedFileNameWithSpaces_IsResolvedVerbatim(string line, string expectedFileName)
    {
        using var reader = new GpssReader(new StringReader(line), "",
            name => name == expectedFileName ? new StringReader(" TERMINATE 1") : throw new FileNotFoundException(name));

        var statement = reader.Read();

        statement!.Verb.ShouldBe("TERMINATE");
        statement.FileName.ShouldBe(expectedFileName);
    }

    [Fact]
    public void Read_Include_UnquotedFileNameWithoutSpaces_StillWorks()
    {
        using var reader = new GpssReader(new StringReader(" INCLUDE child.gpss"), "",
            name => name == "child.gpss" ? new StringReader(" TERMINATE 1") : throw new FileNotFoundException(name));

        reader.Read()!.Verb.ShouldBe("TERMINATE");
    }

    [Fact]
    public void Read_Include_UnterminatedQuote_Throws()
    {
        using var reader = new GpssReader(new StringReader(" INCLUDE \"child.gpss"));

        Should.Throw<FormatException>(() => reader.Read());
    }

    [Fact]
    public void Read_Include_MissingFile_ThrowsWithFileNameAndLineNumberContext()
    {
        const string parentSource = " GENERATE 10\n INCLUDE missing.gpss";

        using var reader = new GpssReader(new StringReader(parentSource), "main.gpss",
            _ => throw new FileNotFoundException("GPSS source file not found. Tried: 'missing.gpss'."));

        reader.Read(); // GENERATE; advances past line 1 so the INCLUDE failure is reported at line 2

        var ex = Should.Throw<FileNotFoundException>(() => reader.Read());
        ex.Message.ShouldBe("'main.gpss' line 2: GPSS source file not found. Tried: 'missing.gpss'.");
    }

    [Fact]
    public void Read_NestedInclude_ResumesEachParentInOrder()
    {
        const string grandparentSource = " INCLUDE parent.gpss\n TERMINATE 1";
        const string parentSource = " INCLUDE child.gpss\n QUEUE Barber";
        const string childSource = " GENERATE 10";

        var files = new Dictionary<string, string>
        {
            ["parent.gpss"] = parentSource,
            ["child.gpss"] = childSource,
        };

        using var reader = new GpssReader(new StringReader(grandparentSource), "grandparent.gpss",
            name => new StringReader(files[name]));

        reader.Read()!.Verb.ShouldBe("GENERATE"); // from child.gpss
        reader.Read()!.Verb.ShouldBe("QUEUE");     // back in parent.gpss
        reader.Read()!.Verb.ShouldBe("TERMINATE"); // back in grandparent.gpss
        reader.Read().ShouldBeNull();
    }

    [Fact]
    public void Read_IncludeWithoutFileNameOperand_Throws()
    {
        using var reader = new GpssReader(new StringReader(" INCLUDE"));

        Should.Throw<FormatException>(() => reader.Read());
    }

    [Fact]
    public void Dispose_DisposesCurrentAndStackedReaders()
    {
        var parentReader = new TrackingReader(" INCLUDE child.gpss");
        var childReader = new TrackingReader(" GENERATE 10");

        var reader = new GpssReader(parentReader, "parent.gpss", _ => childReader);
        reader.Read(); // switches into childReader, pushing parentReader onto the stack

        reader.Dispose();

        parentReader.Disposed.ShouldBeTrue();
        childReader.Disposed.ShouldBeTrue();
    }

    private sealed class TrackingReader(string content) : StringReader(content)
    {
        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
