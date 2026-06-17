using Gpss.Model.Expressions;
using Gpss.Runtime;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class ConstantRandomVariateGeneratorTests
{
    [Theory, InlineData(300)]
    [InlineData(0)]
    public void Sample_ReturnsEvaluatedValue(int value)
    {
        var variate = new ConstantRandomVariateGenerator(new IntegerExpression(value));

        var result = variate.Sample();

        result.ShouldBe(value);
    }
}
