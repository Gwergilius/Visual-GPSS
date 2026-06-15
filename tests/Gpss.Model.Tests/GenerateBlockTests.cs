using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class GenerateBlockTests
{
    [Theory, InlineData(10)]
    public void GenerateBlock_RequiredOperand_MeanInterArrivalTimeIsSet(int value)
    {
        var block = new GenerateBlock(new IntegerExpression(value));

        block.MeanInterArrivalTime.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(value);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_OptionalOperands_DefaultToNull(int value)
    {
        var block = new GenerateBlock(new IntegerExpression(value));

        block.Spread.ShouldBeNull();
        block.FirstTransactionOffset.ShouldBeNull();
        block.GenerationLimit.ShouldBeNull();
        block.Priority.ShouldBeNull();
    }

    [Theory, InlineData("GEN1", 10)]
    public void GenerateBlock_WithLabel_LabelIsPreserved(string label, int value)
    {
        var block = new GenerateBlock(new IntegerExpression(value)) { Label = label };

        block.Label.ShouldBe(label);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_WithoutLabel_LabelIsNull(int value)
    {
        var block = new GenerateBlock(new IntegerExpression(value));

        block.Label.ShouldBeNull();
    }

    [Theory, InlineData(10, 3)]
    public void GenerateBlock_WithSpread_SpreadIsSet(int value, int spread)
    {
        var block = new GenerateBlock(new IntegerExpression(value), Spread: new IntegerExpression(spread));

        block.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_RecordEquality_SameOperandsAreEqual(int value)
    {
        var a = new GenerateBlock(new IntegerExpression(value));
        var b = new GenerateBlock(new IntegerExpression(value));

        a.ShouldBe(b);
    }

    [Theory, InlineData(10, 20)]
    public void GenerateBlock_RecordEquality_DifferentMeanAreNotEqual(int value1, int value2)
    {
        value1.ShouldNotBe(value2, "PRE: Values should be different for this test");
        var a = new GenerateBlock(new IntegerExpression(value1));
        var b = new GenerateBlock(new IntegerExpression(value2));

        a.ShouldNotBe(b);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_IsAssignableToGpssBlock(int value)
    {
        GpssBlock block = new GenerateBlock(new IntegerExpression(value));

        block.ShouldBeAssignableTo<GpssBlock>();
    }
}
