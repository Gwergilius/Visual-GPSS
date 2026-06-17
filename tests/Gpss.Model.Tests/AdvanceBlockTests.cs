using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class AdvanceBlockTests
{
    [Theory, InlineData(400)]
    public void AdvanceBlock_WithMeanDelayTime_TimeIsSet(int mean)
    {
        var block = new AdvanceBlock(new IntegerExpression(mean));

        block.MeanDelayTime.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
    }

    [Fact]
    public void AdvanceBlock_WithoutOperands_OperandsAreNull()
    {
        // ADVANCE with no operand delays by zero
        var block = new AdvanceBlock();

        block.MeanDelayTime.ShouldBeNull();
        block.Spread.ShouldBeNull();
    }

    [Theory, InlineData(400, 200)]
    public void AdvanceBlock_WithSpread_SpreadIsSet(int mean, int spread)
    {
        var block = new AdvanceBlock(new IntegerExpression(mean), Spread: new IntegerExpression(spread));

        block.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData("CUT", 400)]
    public void AdvanceBlock_WithLabel_LabelIsPreserved(string label, int mean)
    {
        var block = new AdvanceBlock(new IntegerExpression(mean)) { Label = label };

        block.Label.ShouldBe(label);
    }

    [Theory, InlineData(400)]
    public void AdvanceBlock_RecordEquality_SameOperandsAreEqual(int mean)
    {
        var a = new AdvanceBlock(new IntegerExpression(mean));
        var b = new AdvanceBlock(new IntegerExpression(mean));

        a.ShouldBe(b);
    }

    [Theory, InlineData(400, 500)]
    public void AdvanceBlock_RecordEquality_DifferentMeanAreNotEqual(int mean1, int mean2)
    {
        mean1.ShouldNotBe(mean2, "PRE: Values should be different for this test");
        var a = new AdvanceBlock(new IntegerExpression(mean1));
        var b = new AdvanceBlock(new IntegerExpression(mean2));

        a.ShouldNotBe(b);
    }

    [Theory, InlineData(400)]
    public void AdvanceBlock_IsAssignableToGpssBlock(int mean)
    {
        GpssBlock block = new AdvanceBlock(new IntegerExpression(mean));

        block.ShouldBeAssignableTo<GpssBlock>();
    }
}
