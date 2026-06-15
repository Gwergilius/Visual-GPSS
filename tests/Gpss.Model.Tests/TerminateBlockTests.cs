using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Model.Tests;

public sealed class TerminateBlockTests
{
    [Fact]
    public void TerminateBlock_WithDecrementCount_CountIsSet()
    {
        var block = new TerminateBlock(null, new IntegerExpression(1));

        var count = Assert.IsType<IntegerExpression>(block.DecrementCount);
        Assert.Equal(1, count.Value);
    }

    [Fact]
    public void TerminateBlock_WithoutDecrementCount_CountIsNull()
    {
        // TERMINATE with no operand passes transactions through without decrementing
        var block = new TerminateBlock(null);

        Assert.Null(block.DecrementCount);
    }

    [Fact]
    public void TerminateBlock_WithLabel_LabelIsPreserved()
    {
        var block = new TerminateBlock("TERM1", new IntegerExpression(1));

        Assert.Equal("TERM1", block.Label);
    }

    [Fact]
    public void TerminateBlock_RecordEquality_SameOperandsAreEqual()
    {
        var a = new TerminateBlock(null, new IntegerExpression(1));
        var b = new TerminateBlock(null, new IntegerExpression(1));

        Assert.Equal(a, b);
    }

    [Fact]
    public void TerminateBlock_RecordEquality_DifferentDecrementCountsAreNotEqual()
    {
        var a = new TerminateBlock(null, new IntegerExpression(1));
        var b = new TerminateBlock(null, new IntegerExpression(2));

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void TerminateBlock_IsAssignableToGpssBlock()
    {
        GpssBlock block = new TerminateBlock(null, new IntegerExpression(1));

        Assert.IsAssignableFrom<GpssBlock>(block);
    }
}
