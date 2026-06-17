using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class GpssBlockTypeExtensionsTests
{
    [Theory]
    [InlineData(typeof(GenerateBlock), "GENERATE")]
    [InlineData(typeof(AdvanceBlock), "ADVANCE")]
    [InlineData(typeof(TerminateBlock), "TERMINATE")]
    [InlineData(typeof(SeizeBlock), "SEIZE")]
    [InlineData(typeof(ReleaseBlock), "RELEASE")]
    [InlineData(typeof(QueueBlock), "QUEUE")]
    [InlineData(typeof(DepartBlock), "DEPART")]
    public void DefaultGpssKeyword_StripsBlockSuffix_AndUpperCases(Type blockType, string expectedKeyword)
    {
        blockType.DefaultGpssKeyword.ShouldBe(expectedKeyword);
    }

    [Fact]
    public void DefaultGpssKeyword_TypeWithoutBlockSuffix_UpperCasesWholeName()
    {
        typeof(SymbolExpression).DefaultGpssKeyword.ShouldBe("SYMBOLEXPRESSION");
    }
}
