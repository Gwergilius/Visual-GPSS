using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Model.Variates;
using Gpss.Parser;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class GpssParserTests
{
    private static readonly GpssParser Parser =
        new ServiceCollection().AddGpssParser().BuildServiceProvider().GetRequiredService<GpssParser>();

    // -------------------------------------------------------------------------
    // Minimal demo program
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_MinimalProgram_ProducesCorrectAst()
    {
        // Unlabeled statements must be indented: the verb is never in column 1.
        const string source = " GENERATE 10\n TERMINATE 1";

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(2);
        result.Program.Blocks[0].ShouldBeOfType<GenerateBlock>()
            .InterArrivalTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(10);
        result.Program.Blocks[1].ShouldBeOfType<TerminateBlock>()
            .DecrementCount.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(1);
    }

    // -------------------------------------------------------------------------
    // GENERATE block
    // -------------------------------------------------------------------------

    [Theory, InlineData(10)]
    public void Parse_GenerateWithMeanOnly_SetsOperandA(int mean)
    {
        var result = Parser.Parse($" GENERATE {mean}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>();
        block.InterArrivalTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        block.FirstTransactionOffset.ShouldBeNull();
        block.GenerationLimit.ShouldBeNull();
        block.Priority.ShouldBeNull();
    }

    [Theory, InlineData(10, 3)]
    public void Parse_GenerateWithSpread_SetsOperandB(int mean, int spread)
    {
        var result = Parser.Parse($" GENERATE {mean},{spread}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>();
        var spec = block.InterArrivalTime.ShouldBeOfType<UniformVariateSpec>();
        spec.Mean.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        spec.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData(10, 2, 5)]
    public void Parse_GenerateWithSkippedOperand_LeavesSlotNull(int mean, int spread, int limit)
    {
        // C operand (offset) is skipped with double-comma
        var result = Parser.Parse($" GENERATE {mean},{spread},,{limit}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>();
        block.FirstTransactionOffset.ShouldBeNull();
        block.GenerationLimit.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(limit);
    }

    // -------------------------------------------------------------------------
    // ADVANCE block
    // -------------------------------------------------------------------------

    [Theory, InlineData(400)]
    public void Parse_AdvanceWithMeanOnly_SetsOperandA(int mean)
    {
        var result = Parser.Parse($" ADVANCE {mean}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<AdvanceBlock>();
        block.DelayTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
    }

    [Theory, InlineData(400, 200)]
    public void Parse_AdvanceWithSpread_SetsOperandB(int mean, int spread)
    {
        var result = Parser.Parse($" ADVANCE {mean},{spread}");

        var block = result.Program!.Blocks[0].ShouldBeOfType<AdvanceBlock>();
        var spec = block.DelayTime.ShouldBeOfType<UniformVariateSpec>();
        spec.Mean.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        spec.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Fact]
    public void Parse_AdvanceWithNoOperand_DelayTimeIsZeroConstant()
    {
        var result = Parser.Parse(" ADVANCE");

        result.Success.ShouldBeTrue();
        var block = result.Program!.Blocks[0].ShouldBeOfType<AdvanceBlock>();
        block.DelayTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(0);
    }

    // -------------------------------------------------------------------------
    // TERMINATE block
    // -------------------------------------------------------------------------

    [Theory, InlineData(1)]
    public void Parse_TerminateWithDecrement_SetsOperandA(int decrement)
    {
        var result = Parser.Parse($" TERMINATE {decrement}");

        result.Program!.Blocks[0].ShouldBeOfType<TerminateBlock>()
            .DecrementCount.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(decrement);
    }

    [Fact]
    public void Parse_TerminateWithNoOperand_DecrementIsNull()
    {
        var result = Parser.Parse(" TERMINATE");

        result.Program!.Blocks[0].ShouldBeOfType<TerminateBlock>()
            .DecrementCount.ShouldBeNull();
    }

    // -------------------------------------------------------------------------
    // Labels
    // -------------------------------------------------------------------------

    [Theory, InlineData("GEN1", 10)]
    public void Parse_BlockWithLabel_LabelIsPreserved(string label, int mean)
    {
        // The label occupies column 1 (no leading whitespace); that is what marks it as a label.
        var result = Parser.Parse($"{label} GENERATE {mean}");

        result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>().Label.ShouldBe(label);
    }

    // -------------------------------------------------------------------------
    // QUEUE and DEPART blocks
    // -------------------------------------------------------------------------

    [Theory, InlineData("Barber")]
    public void Parse_Queue_ProducesQueueBlockWithSymbolOperand(string queueName)
    {
        var result = Parser.Parse($" QUEUE {queueName}");

        result.Success.ShouldBeTrue();
        result.Program!.Blocks[0].ShouldBeOfType<QueueBlock>()
            .QueueName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(queueName);
    }

    [Theory, InlineData("Barber")]
    public void Parse_Depart_ProducesDepartBlockWithSymbolOperand(string queueName)
    {
        var result = Parser.Parse($" DEPART {queueName}");

        result.Success.ShouldBeTrue();
        result.Program!.Blocks[0].ShouldBeOfType<DepartBlock>()
            .QueueName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(queueName);
    }

    // -------------------------------------------------------------------------
    // SEIZE and RELEASE blocks
    // -------------------------------------------------------------------------

    [Theory, InlineData("Barber")]
    public void Parse_Seize_ProducesSeizeBlockWithSymbolOperand(string facilityName)
    {
        var result = Parser.Parse($" SEIZE {facilityName}");

        result.Success.ShouldBeTrue();
        result.Program!.Blocks[0].ShouldBeOfType<SeizeBlock>()
            .FacilityName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(facilityName);
    }

    [Theory, InlineData("Barber")]
    public void Parse_Release_ProducesReleaseBlockWithSymbolOperand(string facilityName)
    {
        var result = Parser.Parse($" RELEASE {facilityName}");

        result.Success.ShouldBeTrue();
        result.Program!.Blocks[0].ShouldBeOfType<ReleaseBlock>()
            .FacilityName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(facilityName);
    }

    [Fact]
    public void Parse_SeizeWithoutOperand_ReturnsError()
    {
        var result = Parser.Parse(" SEIZE");

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_ReleaseWithoutOperand_ReturnsError()
    {
        var result = Parser.Parse(" RELEASE");

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_BuilderDiagnostics_CarryFileNameAndLineNumber()
    {
        const string source = " GENERATE 10\n SEIZE";

        var result = Parser.Parse(source, "main.gpss");

        var diagnostic = result.Diagnostics.Single(d => d.Severity == DiagnosticSeverity.Error);
        diagnostic.FileName.ShouldBe("main.gpss");
        diagnostic.LineNumber.ShouldBe(2);
        diagnostic.ToString().ShouldBe("[Error] main.gpss(2): SEIZE requires operand A (facility name).");
    }

    [Fact]
    public void Parse_OperandParsingDiagnostics_CarryFileNameAndLineNumber()
    {
        const string source = " GENERATE 10\n ADVANCE ABC";

        var result = Parser.Parse(source, "main.gpss");

        var diagnostic = result.Diagnostics.Single(d => d.Severity == DiagnosticSeverity.Error);
        diagnostic.FileName.ShouldBe("main.gpss");
        diagnostic.LineNumber.ShouldBe(2);
        diagnostic.ToString().ShouldBe("[Error] main.gpss(2): operand A 'ABC' is not a valid integer.");
    }

    // -------------------------------------------------------------------------
    // Comments and whitespace
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_StarComment_IsIgnored()
    {
        const string source = "*****************************\n"
            + "* Barber Shop Simulation    *\n"
            + "*****************************\n"
            + " GENERATE 10\n"
            + " TERMINATE 1";

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(2);
    }

    [Fact]
    public void Parse_InlineComment_IsIgnored()
    {
        var result = Parser.Parse(" GENERATE 10  ; creates a customer every 10 time units");

        result.Success.ShouldBeTrue();
        result.Program!.Blocks[0].ShouldBeOfType<GenerateBlock>()
            .InterArrivalTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(10);
    }

    [Fact]
    public void Parse_InlineComment_IsCarriedOntoTheBuiltBlockAsDescription()
    {
        var result = Parser.Parse(" GENERATE 10  ; creates a customer every 10 time units");

        result.Program!.Blocks[0].Description.ShouldBe("creates a customer every 10 time units");
    }

    [Fact]
    public void Parse_InlineComment_IsLoggedAsInfoDiagnostic()
    {
        var result = Parser.Parse(" GENERATE 10  ; creates a customer every 10 time units");

        result.Diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Info &&
            d.Message.Contains("creates a customer every 10 time units"));
    }

    [Fact]
    public void Parse_Diagnostics_CarryFileNameAndLineNumber()
    {
        const string source = " GENERATE 10\n FOOBAR 42";

        var result = Parser.Parse(source, "main.gpss");

        var diagnostic = result.Diagnostics.Single(d => d.Severity == DiagnosticSeverity.Error);
        diagnostic.FileName.ShouldBe("main.gpss");
        diagnostic.LineNumber.ShouldBe(2);
        diagnostic.ToString().ShouldBe("[Error] main.gpss(2): 'FOOBAR' is not a recognised block name.");
    }

    [Fact]
    public void Parse_ErrorOnCommentedLine_DiagnosticIncludesTheComment()
    {
        const string source = " GENERATE 10\n FOOBAR 42  ; what is this supposed to do?";

        var result = Parser.Parse(source, "main.gpss");

        var diagnostic = result.Diagnostics.Single(d => d.Severity == DiagnosticSeverity.Error);
        diagnostic.Comment.ShouldBe("what is this supposed to do?");
        diagnostic.ToString().ShouldBe(
            "[Error] main.gpss(2): 'FOOBAR' is not a recognised block name. (what is this supposed to do?)");
    }

    // -------------------------------------------------------------------------
    // INCLUDE
    // -------------------------------------------------------------------------

    [Fact]
    public void Parse_Include_ReadsBlocksFromIncludedFile()
    {
        var tempDir = Directory.CreateTempSubdirectory();
        try
        {
            File.WriteAllText(Path.Combine(tempDir.FullName, "included.gpss"), " TERMINATE 1");

            const string source = " GENERATE 10\n INCLUDE included.gpss";

            var result = Parser.Parse(source, Path.Combine(tempDir.FullName, "main.gpss"));

            result.Success.ShouldBeTrue();
            result.Program!.Blocks.Count.ShouldBe(2);
            result.Program.Blocks[0].ShouldBeOfType<GenerateBlock>();
            result.Program.Blocks[1].ShouldBeOfType<TerminateBlock>();
        }
        finally
        {
            tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void Parse_EmptyAndWhitespaceLines_AreIgnored()
    {
        const string source = "\n GENERATE 10\n\n TERMINATE 1\n";

        var result = Parser.Parse(source);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.Count.ShouldBe(2);
    }

    [Fact]
    public void Parse_EndStatement_StopsParsingFurtherBlocks()
    {
        const string source = " GENERATE 10\n END\n TERMINATE 1";

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
        var result = Parser.Parse(" FOOBAR 42");

        result.Success.ShouldBeFalse();
        result.Program.ShouldBeNull();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Theory, InlineData(" GENERATE ABC")]
    public void Parse_NonIntegerOperand_ReturnsError(string source)
    {
        var result = Parser.Parse(source);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_GenerateWithoutOperandA_ReturnsError()
    {
        var result = Parser.Parse(" GENERATE");

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Parse_FlushLeftUnlabeledStatement_IsRejectedAsUnknownBlockName()
    {
        // The verb is never in column 1: a flush-left, unindented, label-less line like
        // "TERMINATE 1" is read as label "TERMINATE" + verb "1", which is rejected here as an
        // unrecognised block name rather than being specially detected and explained.
        var result = Parser.Parse("TERMINATE 1");

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d =>
            d.Severity == DiagnosticSeverity.Error && d.Message.Contains("'1'"));
    }

    [Fact]
    public void Parse_EmptySource_ReturnsEmptyProgram()
    {
        var result = Parser.Parse(string.Empty);

        result.Success.ShouldBeTrue();
        result.Program!.Blocks.ShouldBeEmpty();
    }
}
