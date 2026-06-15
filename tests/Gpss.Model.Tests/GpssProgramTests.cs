using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Model.Tests;

public sealed class GpssProgramTests
{
    [Fact]
    public void Program_WithGenerateAndTerminate_ContainsBothBlocksInOrder()
    {
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new TerminateBlock(new IntegerExpression(1))
        ]);

        Assert.Equal(2, program.Blocks.Count);
        Assert.IsType<GenerateBlock>(program.Blocks[0]);
        Assert.IsType<TerminateBlock>(program.Blocks[1]);
    }

    [Fact]
    public void Program_RecordEquality_TwoProgramsWithSameBlocksAreEqual()
    {
        var a = new GpssProgram([new GenerateBlock(new IntegerExpression(10))]);
        var b = new GpssProgram([new GenerateBlock(new IntegerExpression(10))]);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Program_RecordEquality_ProgramsWithDifferentBlocksAreNotEqual()
    {
        var a = new GpssProgram([new GenerateBlock(new IntegerExpression(10))]);
        var b = new GpssProgram([new GenerateBlock(new IntegerExpression(99))]);

        Assert.NotEqual(a, b);
    }
}
