using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class GpssProgramTests
{
    [Theory, InlineData(10, 1)]
    public void Program_WithGenerateAndTerminate_ContainsBothBlocksInOrder(int meanArrival, int decrementCount)
    {
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(meanArrival)),
            new TerminateBlock(new IntegerExpression(decrementCount))
        ]);

        program.Blocks.Count.ShouldBe(2);
        program.Blocks[0].ShouldBeOfType<GenerateBlock>();
        program.Blocks[1].ShouldBeOfType<TerminateBlock>();
    }

    [Theory, InlineData(10)]
    public void Program_RecordEquality_TwoProgramsWithSameBlocksAreEqual(int meanArrival)
    {
        var a = new GpssProgram([new GenerateBlock(new IntegerExpression(meanArrival))]);
        var b = new GpssProgram([new GenerateBlock(new IntegerExpression(meanArrival))]);

        a.ShouldBe(b);
    }

    [Theory, InlineData(10, 99)]
    public void Program_RecordEquality_ProgramsWithDifferentBlocksAreNotEqual(int meanA, int meanB)
    {
        meanA.ShouldNotBe(meanB);
        var a = new GpssProgram([new GenerateBlock(new IntegerExpression(meanA))]);
        var b = new GpssProgram([new GenerateBlock(new IntegerExpression(meanB))]);

        a.ShouldNotBe(b);
    }
}
