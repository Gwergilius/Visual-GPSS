using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Parser;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class GpssParserTests
{
    private static readonly GpssParser Parser = new();

    // -------------------------------------------------------------------------
    // Minimal demo program
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_MinimalProgram_ProducesCorrectAst()
    {
        const string source = """
              GENERATE 10
              TERMINATE 1
            """;

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(2);
        result.Program.Blocks[0].ShouldBeOfType<GenerateBlock>()
            .MeanInterArrivalTime.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(10);
        result.Program.Blocks[1].ShouldBeOfType<TerminateBlock>()
            .DecrementCount.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(1);
    }

    // -------------------------------------------------------------------------
    // GENERATE block
    // -------------------------------------------------------------------------

    [Theory, InlineData(10)]
    public void Parse_GenerateWithMeanOnly_SetsOperandA(int mean)
    {
        var result = Parser.Parse($"GENERATE {mean}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>();
        block.MeanInterArrivalTime.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        block.Spread.ShouldBeNull();
        block.FirstTransactionOffset.ShouldBeNull();
        block.GenerationLimit.ShouldBeNull();
        block.Priority.ShouldBeNull();
    }

    [Theory, InlineData(10, 3)]
    public void Parse_GenerateWithSpread_SetsOperandB(int mean, int spread)
    {
        var result = Parser.Parse($"GENERATE {mean},{spread}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>();
        block.MeanInterArrivalTime.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        block.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData(10, 2, 5)]
    public void Parse_GenerateWithSkippedOperand_LeavesSlotNull(int mean, int spread, int limit)
    {
        // C operand (offset) is skipped with double-comma
        var result = Parser.Parse($"GENERATE {mean},{spread},,{limit}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>();
        block.FirstTransactionOffset.ShouldBeNull();
        block.GenerationLimit.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(limit);
    }

    // -------------------------------------------------------------------------
    // TERMINATE block
    // -------------------------------------------------------------------------

    [Theory, InlineData(1)]
    public void Parse_TerminateWithDecrement_SetsOperandA(int decrement)
    {
        var result = Parser.Parse($"TERMINATE {decrement}");

        result.Program!.Blocks[0].ShouldBeOfType<TerminateBlock>()
            .DecrementCount.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(decrement);
    }

    [Fact]
    public void Parse_TerminateWithNoOperand_DecrementIsNull()
    {
        var result = Parser.Parse("TERMINATE");

        result.Program!.Blocks[0].ShouldBeOfType<TerminateBlock>()
            .DecrementCount.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Labels
    // -------------------------------------------------------------------------

    [Theory, InlineData("GEN1", 10)]
    public void Parse_BlockWithLabel_LabelIsPreserved(string label, int mean)
    {
        var result = Parser.Parse($"{label} GENERATE {mean}");

        result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>().Label.ShouldBe(label);
    }

    // -------------------------------------------------------------------------
    // Comments and whitespace
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_StarComment_IsIgnored()
    {
        const string source = """
            *****************************
            * Barber Shop Simulation    *
            *****************************
            GENERATE 10
            TERMINATE 1
            """;

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(2);
    }

    [Fact]
    public void Parse_InlineComment_IsIgnored()
    {
        var result = Parser.Parse("GENERATE 10  ; creates a customer every 10 time units");

        result.Success.ShouldBeTrue();
        result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>()
            .MeanInterArrivalTime.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(10);
    }

    [Fact]
    public void Parse_EmptyAndWhitespaceLines_AreIgnored()
    {
        const string source = """

              GENERATE 10

              TERMINATE 1

            """;

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(2);
    }

    [Fact]
    public void Parse_EndStatement_StopsParsingFurtherBlocks()
    {
        const string source = """
            GENERATE 10
            END
            TERMINATE 1
            """;

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(1);
        result.Program.Blocks[0].ShouldBeOfType<GenerateBlock>();
    }

    // -------------------------------------------------------------------------
    // Error cases
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_UnknownBlockName_ReturnsError()
    {
        var result = Parser.Parse("SEIZE CASHIER");

        result.Success.ShouldBeFalse();
        result.Program.ShouldBeNull();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Theory, InlineData("GENERATE ABC")]
    public void Parse_NonIntegerOperand_ReturnsError(string source)
    {
        var result = Parser.Parse(source);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_GenerateWithoutOperandA_ReturnsError()
    {
        var result = Parser.Parse("GENERATE");

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_EmptySource_ReturnsEmptyProgram()
    {
        var result = Parser.Parse(string.Empty);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.ShouldBeEmpty();
    }
}
