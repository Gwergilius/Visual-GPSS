using Gpss.Model.Expressions;
using Gpss.Runtime;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class UniformRandomVariateGeneratorTests
{
    [Theory]
    [InlineData(300, 100, 350)]
    [InlineData(50, 10, 45)]
    public void Sample_WithNonZeroSpread_DelegatesToNextIntWithMeanPlusMinusSpreadBounds(
        int mean, int spread, int stubbedResult)
    {
        var stub = new RecordingRandomNumberGenerator(stubbedResult);
        var variate = new UniformRandomVariateGenerator(stub, new IntegerExpression(mean), new IntegerExpression(spread));

        var result = variate.Sample();

        result.ShouldBe(stubbedResult);
        stub.LastMin.ShouldBe(mean - spread);
        stub.LastMax.ShouldBe(mean + spread);
    }

    [Theory, InlineData(300)]
    [InlineData(0)]
    public void Sample_WithZeroSpread_ReturnsMeanWithoutCallingGenerator(int mean)
    {
        var stub = new RecordingRandomNumberGenerator(stubbedResult: -1);
        var variate = new UniformRandomVariateGenerator(stub, new IntegerExpression(mean), new IntegerExpression(0));

        var result = variate.Sample();

        result.ShouldBe(mean);
        stub.WasCalled.ShouldBeFalse();
    }

    [Theory, InlineData(300, 100)]
    public void Sample_ReevaluatesExpressionsOnEveryCall(int mean, int spread)
    {
        var stub = new RecordingRandomNumberGenerator(stubbedResult: mean);
        var meanExpression = new IntegerExpression(mean);
        var variate = new UniformRandomVariateGenerator(stub, meanExpression, new IntegerExpression(spread));

        variate.Sample();
        variate.Sample();

        stub.CallCount.ShouldBe(2);
    }

    private sealed class RecordingRandomNumberGenerator(int stubbedResult) : IRandomNumberGenerator
    {
        internal bool WasCalled { get; private set; }
        internal int CallCount { get; private set; }
        internal int LastMin { get; private set; }
        internal int LastMax { get; private set; }

        public double NextDouble() => throw new NotSupportedException();
        public double NextUniform(double min, double max) => throw new NotSupportedException();

        public int NextInt(int min, int max)
        {
            WasCalled = true;
            CallCount++;
            LastMin = min;
            LastMax = max;
            return stubbedResult;
        }
    }
}
