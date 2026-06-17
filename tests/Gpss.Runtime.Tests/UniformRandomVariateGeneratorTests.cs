using Gpss.Runtime;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class UniformRandomVariateGeneratorTests
{
    [Theory]
    [InlineData(300, 100, 350)]
    [InlineData(50, 10, 45)]
    public void Sample_WithNonZeroSpread_DelegatesToNextIntWithMeanPlusMinusSpreadBounds(
        double mean, double spread, int stubbedResult)
    {
        var stub = new RecordingRandomNumberGenerator(stubbedResult);
        var variate = new UniformRandomVariateGenerator(stub);

        var result = variate.Sample(mean, spread);

        result.ShouldBe(stubbedResult);
        stub.LastMin.ShouldBe((int)(mean - spread));
        stub.LastMax.ShouldBe((int)(mean + spread));
    }

    [Theory, InlineData(300)]
    [InlineData(0)]
    public void Sample_WithZeroSpread_ReturnsMeanWithoutCallingGenerator(double mean)
    {
        var stub = new RecordingRandomNumberGenerator(stubbedResult: -1);
        var variate = new UniformRandomVariateGenerator(stub);

        var result = variate.Sample(mean, spread: 0);

        result.ShouldBe(mean);
        stub.WasCalled.ShouldBeFalse();
    }

    private sealed class RecordingRandomNumberGenerator(int stubbedResult) : IRandomNumberGenerator
    {
        internal bool WasCalled { get; private set; }
        internal int LastMin { get; private set; }
        internal int LastMax { get; private set; }

        public double NextDouble() => throw new NotSupportedException();
        public double NextUniform(double min, double max) => throw new NotSupportedException();

        public int NextInt(int min, int max)
        {
            WasCalled = true;
            LastMin = min;
            LastMax = max;
            return stubbedResult;
        }
    }
}
