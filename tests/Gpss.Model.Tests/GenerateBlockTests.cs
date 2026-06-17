using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Model.Variates;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class GenerateBlockTests
{
    [Theory, InlineData(10)]
    public void GenerateBlock_RequiredOperand_InterArrivalTimeIsSet(int value)
    {
        var block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));

        block.InterArrivalTime.ShouldBeOfType<ConstantVariateSpec>()
            .Value.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(value);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_OptionalOperands_DefaultToNull(int value)
    {
        var block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));

        block.FirstTransactionOffset.ShouldBeNull();
        block.GenerationLimit.ShouldBeNull();
        block.Priority.ShouldBeNull();
    }

    [Theory, InlineData("GEN1", 10)]
    public void GenerateBlock_WithLabel_LabelIsPreserved(string label, int value)
    {
        var block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value))) { Label = label };

        block.Label.ShouldBe(label);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_WithoutLabel_LabelIsNull(int value)
    {
        var block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));

        block.Label.ShouldBeNull();
    }

    [Theory, InlineData("Create next customer", 10)]
    public void GenerateBlock_WithDescription_DescriptionIsPreserved(string description, int value)
    {
        var block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value))) { Description = description };

        block.Description.ShouldBe(description);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_WithoutDescription_DescriptionIsNull(int value)
    {
        var block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));

        block.Description.ShouldBeNull();
    }

    [Theory, InlineData(10, 3)]
    public void GenerateBlock_WithSpread_InterArrivalTimeIsUniform(int mean, int spread)
    {
        var block = new GenerateBlock(VariateSpec.Uniform(new IntegerExpression(mean), new IntegerExpression(spread)));

        var spec = block.InterArrivalTime.ShouldBeOfType<UniformVariateSpec>();
        spec.Mean.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(mean);
        spec.Spread.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(spread);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_RecordEquality_SameOperandsAreEqual(int value)
    {
        var a = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));
        var b = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));

        a.ShouldBe(b);
    }

    [Theory, InlineData(10, 20)]
    public void GenerateBlock_RecordEquality_DifferentMeanAreNotEqual(int value1, int value2)
    {
        value1.ShouldNotBe(value2, "PRE: Values should be different for this test");
        var a = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value1)));
        var b = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value2)));

        a.ShouldNotBe(b);
    }

    [Theory, InlineData(10)]
    public void GenerateBlock_IsAssignableToGpssBlock(int value)
    {
        GpssBlock block = new GenerateBlock(VariateSpec.Constant(new IntegerExpression(value)));

        block.ShouldBeAssignableTo<GpssBlock>();
    }
}
