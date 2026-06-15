using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Model.Tests;

public sealed class GenerateBlockTests
{
    [Fact]
    public void GenerateBlock_RequiredOperand_MeanInterArrivalTimeIsSet()
    {
        var block = new GenerateBlock(null, new IntegerExpression(10));

        var mean = Assert.IsType<IntegerExpression>(block.MeanInterArrivalTime);
        Assert.Equal(10, mean.Value);
    }

    [Fact]
    public void GenerateBlock_OptionalOperands_DefaultToNull()
    {
        var block = new GenerateBlock(null, new IntegerExpression(10));

        Assert.Null(block.Spread);
        Assert.Null(block.FirstTransactionOffset);
        Assert.Null(block.GenerationLimit);
        Assert.Null(block.Priority);
    }

    [Fact]
    public void GenerateBlock_WithLabel_LabelIsPreserved()
    {
        var block = new GenerateBlock("GEN1", new IntegerExpression(10));

        Assert.Equal("GEN1", block.Label);
    }

    [Fact]
    public void GenerateBlock_WithoutLabel_LabelIsNull()
    {
        var block = new GenerateBlock(null, new IntegerExpression(10));

        Assert.Null(block.Label);
    }

    [Fact]
    public void GenerateBlock_WithSpread_SpreadIsSet()
    {
        var block = new GenerateBlock(null, new IntegerExpression(10), Spread: new IntegerExpression(3));

        var spread = Assert.IsType<IntegerExpression>(block.Spread);
        Assert.Equal(3, spread.Value);
    }

    [Fact]
    public void GenerateBlock_RecordEquality_SameOperandsAreEqual()
    {
        var a = new GenerateBlock(null, new IntegerExpression(10));
        var b = new GenerateBlock(null, new IntegerExpression(10));

        Assert.Equal(a, b);
    }

    [Fact]
    public void GenerateBlock_RecordEquality_DifferentMeanAreNotEqual()
    {
        var a = new GenerateBlock(null, new IntegerExpression(10));
        var b = new GenerateBlock(null, new IntegerExpression(20));

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void GenerateBlock_IsAssignableToGpssBlock()
    {
        GpssBlock block = new GenerateBlock(null, new IntegerExpression(10));

        Assert.IsAssignableFrom<GpssBlock>(block);
    }
}
