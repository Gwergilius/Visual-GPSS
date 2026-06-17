using Gpss.Model.Expressions;
using Gpss.Model.Variates;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class VariateSpecTests
{
    [Theory, InlineData(10)]
    public void ConstantVariateSpec_ValueIsSet(int value)
    {
        var spec = new ConstantVariateSpec(new IntegerExpression(value));

        spec.Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(value);
    }

    [Theory, InlineData(10, 3)]
    public void UniformVariateSpec_MeanAndSpreadAreSet(int mean, int spread)
    {
        var spec = new UniformVariateSpec(new IntegerExpression(mean), new IntegerExpression(spread));

        spec.Mean.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        spec.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData(10)]
    public void Constant_FactoryMethod_ReturnsConstantVariateSpec(int value)
    {
        var spec = VariateSpec.Constant(new IntegerExpression(value));

        spec.ShouldBeOfType<ConstantVariateSpec>().Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(value);
    }

    [Theory, InlineData(10, 3)]
    public void Uniform_FactoryMethod_ReturnsUniformVariateSpec(int mean, int spread)
    {
        var spec = VariateSpec.Uniform(new IntegerExpression(mean), new IntegerExpression(spread));

        var uniform = spec.ShouldBeOfType<UniformVariateSpec>();
        uniform.Mean.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        uniform.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData(10)]
    public void ConstantVariateSpec_RecordEquality_SameValueAreEqual(int value)
    {
        var a = new ConstantVariateSpec(new IntegerExpression(value));
        var b = new ConstantVariateSpec(new IntegerExpression(value));

        a.ShouldBe(b);
    }

    [Theory, InlineData(10, 20)]
    public void ConstantVariateSpec_RecordEquality_DifferentValueAreNotEqual(int value1, int value2)
    {
        value1.ShouldNotBe(value2, "PRE: Values should be different for this test");
        var a = new ConstantVariateSpec(new IntegerExpression(value1));
        var b = new ConstantVariateSpec(new IntegerExpression(value2));

        a.ShouldNotBe(b);
    }

    [Theory, InlineData(10, 3)]
    public void UniformVariateSpec_RecordEquality_SameOperandsAreEqual(int mean, int spread)
    {
        var a = new UniformVariateSpec(new IntegerExpression(mean), new IntegerExpression(spread));
        var b = new UniformVariateSpec(new IntegerExpression(mean), new IntegerExpression(spread));

        a.ShouldBe(b);
    }

    [Theory, InlineData(10)]
    public void ConstantVariateSpec_IsAssignableToVariateSpec(int value)
    {
        VariateSpec spec = new ConstantVariateSpec(new IntegerExpression(value));

        spec.ShouldBeAssignableTo<VariateSpec>();
    }
}
