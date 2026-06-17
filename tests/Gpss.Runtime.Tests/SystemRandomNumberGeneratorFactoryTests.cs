using Gpss.Model.Expressions;
using Gpss.Model.Variates;
using Gpss.Runtime;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class SystemRandomNumberGeneratorFactoryTests
{
    [Theory, InlineData(42)]
    public void Create_SameStream_PoolsAndContinuesTheSameSequence(int seed)
    {
        var factory = new SystemRandomNumberGeneratorFactory(seed);
        var reference = new Random(seed);
        var spec = new UniformVariateSpec(new IntegerExpression(100), new IntegerExpression(50));

        var first = factory.Create(spec, 1).Sample();
        var second = factory.Create(spec, 1).Sample();

        first.ShouldBe(reference.Next(50, 151));
        second.ShouldBe(reference.Next(50, 151));
    }

    [Theory, InlineData(42)]
    public void Create_DifferentStreams_AreIndependentSequences(int seed)
    {
        var factory = new SystemRandomNumberGeneratorFactory(seed);
        var referenceStream1 = new Random(seed);
        var referenceStream2 = new Random(seed + 1);
        var spec = new UniformVariateSpec(new IntegerExpression(100), new IntegerExpression(50));

        var stream1Value = factory.Create(spec, 1).Sample();
        var stream2Value = factory.Create(spec, 2).Sample();

        stream1Value.ShouldBe(referenceStream1.Next(50, 151));
        stream2Value.ShouldBe(referenceStream2.Next(50, 151));
    }

    [Theory, InlineData(7)]
    public void Create_ConstantSpec_ReturnsValueWithoutConsumingTheStream(int value)
    {
        var factory = new SystemRandomNumberGeneratorFactory(seed: 42);
        var spec = new ConstantVariateSpec(new IntegerExpression(value));

        var result = factory.Create(spec).Sample();

        result.ShouldBe(value);
    }
}
