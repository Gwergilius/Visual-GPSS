using Gpss.Runtime;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class SystemRandomNumberGeneratorFactoryTests
{
    [Theory, InlineData(42)]
    public void CreateUniform_SameStream_PoolsAndContinuesTheSameSequence(int seed)
    {
        var factory = new SystemRandomNumberGeneratorFactory(seed);
        var reference = new Random(seed);

        var first = factory.CreateUniform(1).Sample(100, 50);
        var second = factory.CreateUniform(1).Sample(100, 50);

        first.ShouldBe(reference.Next(50, 151));
        second.ShouldBe(reference.Next(50, 151));
    }

    [Theory, InlineData(42)]
    public void CreateUniform_DifferentStreams_AreIndependentSequences(int seed)
    {
        var factory = new SystemRandomNumberGeneratorFactory(seed);
        var referenceStream1 = new Random(seed);
        var referenceStream2 = new Random(seed + 1);

        var stream1Value = factory.CreateUniform(1).Sample(100, 50);
        var stream2Value = factory.CreateUniform(2).Sample(100, 50);

        stream1Value.ShouldBe(referenceStream1.Next(50, 151));
        stream2Value.ShouldBe(referenceStream2.Next(50, 151));
    }
}
