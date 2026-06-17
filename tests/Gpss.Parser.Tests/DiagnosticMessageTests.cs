using Gpss.Contracts;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class DiagnosticMessageTests
{
    [Fact]
    public void ToString_NoLocation_RendersSeverityAndMessageOnly()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "boom");

        message.ToString().ShouldBe("[Error] boom");
    }

    [Fact]
    public void ToString_LineNumberOnly_IncludesLinePrefix()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Warning, "boom", LineNumber: 5);

        message.ToString().ShouldBe("[Warning] Line 5: boom");
    }

    [Fact]
    public void ToString_FileNameAndLineNumber_IncludesBoth()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Info, "boom", "main.gpss", 5);

        message.ToString().ShouldBe("[Info] main.gpss(5): boom");
    }

    [Fact]
    public void ToString_FileNameOnly_OmitsLine()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Info, "boom", FileName: "main.gpss");

        message.ToString().ShouldBe("[Info] main.gpss: boom");
    }

    [Fact]
    public void Constructor_FromSourceLocation_CopiesFileNameAndLineNumber()
    {
        var statement = new GpssStatement("main.gpss", 1, 5, null, "GENERATE", [], null);

        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "boom", statement);

        message.FileName.ShouldBe("main.gpss");
        message.LineNumber.ShouldBe(5);
        message.ToString().ShouldBe("[Error] main.gpss(5): boom");
    }

    [Fact]
    public void Constructor_FromSourceLocation_CopiesComment()
    {
        var statement = new GpssStatement("main.gpss", 1, 5, null, "GENERATE", [], "Create next customer");

        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "boom", statement);

        message.Comment.ShouldBe("Create next customer");
        message.ToString().ShouldBe("[Error] main.gpss(5): boom (Create next customer)");
    }

    [Fact]
    public void ToString_CommentOnly_AppendsItInParentheses()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "boom", Comment: "Create next customer");

        message.ToString().ShouldBe("[Error] boom (Create next customer)");
    }

    [Fact]
    public void ToString_NoComment_OmitsParentheses()
    {
        var message = new DiagnosticMessage(DiagnosticSeverity.Error, "boom", "main.gpss", 5);

        message.ToString().ShouldBe("[Error] main.gpss(5): boom");
    }
}
