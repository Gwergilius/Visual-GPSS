using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Model.Variates;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class AdvanceBlockTests
{
    [Theory, InlineData(400)]
    public void AdvanceBlock_WithDelayTime_TimeIsSet(int mean)
    {
        var block = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean)));

        block.DelayTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
    }

    [Fact]
    public void AdvanceBlock_WithZeroConstant_DelayTimeIsZero()
    {
        // ADVANCE with no operand delays by zero
        var block = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(0)));

        block.DelayTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(0);
    }

    [Theory, InlineData(400, 200)]
    public void AdvanceBlock_WithSpread_DelayTimeIsUniform(int mean, int spread)
    {
        var block = new AdvanceBlock(VariateSpec.Uniform(new IntegerExpression(mean), new IntegerExpression(spread)));

        var spec = block.DelayTime.ShouldBeOfType<UniformVariateSpec>();
        spec.Mean.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        spec.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData("CUT", 400)]
    public void AdvanceBlock_WithLabel_LabelIsPreserved(string label, int mean)
    {
        var block = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean))) { Label = label };

        block.Label.ShouldBe(label);
    }

    [Theory, InlineData(400)]
    public void AdvanceBlock_RecordEquality_SameOperandsAreEqual(int mean)
    {
        var a = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean)));
        var b = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean)));

        a.ShouldBe(b);
    }

    [Theory, InlineData(400, 500)]
    public void AdvanceBlock_RecordEquality_DifferentMeanAreNotEqual(int mean1, int mean2)
    {
        mean1.ShouldNotBe(mean2, "PRE: Values should be different for this test");
        var a = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean1)));
        var b = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean2)));

        a.ShouldNotBe(b);
    }

    [Theory, InlineData(400)]
    public void AdvanceBlock_IsAssignableToGpssBlock(int mean)
    {
        GpssBlock block = new AdvanceBlock(VariateSpec.Constant(new IntegerExpression(mean)));

        block.ShouldBeAssignableTo<GpssBlock>();
    }
}
