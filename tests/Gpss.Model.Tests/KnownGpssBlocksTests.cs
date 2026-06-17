using Gpss.Model.Blocks;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class KnownGpssBlocksTests
{
    [Theory]
    [InlineData("GENERATE", typeof(GenerateBlock))]
    [InlineData("generate", typeof(GenerateBlock))]
    [InlineData("ADVANCE", typeof(AdvanceBlock))]
    [InlineData("TERMINATE", typeof(TerminateBlock))]
    [InlineData("SEIZE", typeof(SeizeBlock))]
    [InlineData("RELEASE", typeof(ReleaseBlock))]
    [InlineData("QUEUE", typeof(QueueBlock))]
    [InlineData("DEPART", typeof(DepartBlock))]
    public void ByName_KnownKeyword_MapsToExpectedBlockType(string keyword, Type expectedType)
    {
        KnownGpssBlocks.IsKnown(keyword).ShouldBeTrue();
        KnownGpssBlocks.ByName[keyword].ShouldBe(expectedType);
    }

    [Theory, InlineData("SPLIT")]
    public void IsKnown_UnrecognisedKeyword_ReturnsFalse(string keyword)
    {
        KnownGpssBlocks.IsKnown(keyword).ShouldBeFalse();
    }

    [Fact]
    public void ByName_EveryEntry_IsAGpssBlockSubtype()
    {
        KnownGpssBlocks.ByName.Values.ShouldAllBe(t => typeof(GpssBlock).IsAssignableFrom(t));
    }
}
