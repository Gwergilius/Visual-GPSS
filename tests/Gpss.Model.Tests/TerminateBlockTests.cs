using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class TerminateBlockTests
{
    [Theory, InlineData(1)]
    public void TerminateBlock_WithDecrementCount_CountIsSet(int count)
    {
        var block = new TerminateBlock(new IntegerExpression(count));

        block.DecrementCount.ShouldBeOfType<IntegerExpression>().Value.ShouldBe(count);
    }

    [Fact]
    public void TerminateBlock_WithoutDecrementCount_CountIsNull()
    {
        // TERMINATE with no operand destroys the transaction without decrementing the counter
        var block = new TerminateBlock();

        block.DecrementCount.ShouldBeNull();
    }

    [Theory, InlineData("TERM1", 1)]
    public void TerminateBlock_WithLabel_LabelIsPreserved(string label, int count)
    {
        var block = new TerminateBlock(new IntegerExpression(count)) { Label = label };

        block.Label.ShouldBe(label);
    }

    [Theory, InlineData(1)]
    public void TerminateBlock_RecordEquality_SameOperandsAreEqual(int count)
    {
        var a = new TerminateBlock(new IntegerExpression(count));
        var b = new TerminateBlock(new IntegerExpression(count));

        a.ShouldBe(b);
    }

    [Theory, InlineData(1, 2)]
    public void TerminateBlock_RecordEquality_DifferentDecrementCountsAreNotEqual(int count1, int count2)
    {
        count1.ShouldNotBe(count2, "PRE: Values should be different for this test");
        var a = new TerminateBlock(new IntegerExpression(count1));
        var b = new TerminateBlock(new IntegerExpression(count2));

        a.ShouldNotBe(b);
    }

    [Theory, InlineData(1)]
    public void TerminateBlock_IsAssignableToGpssBlock(int count)
    {
        GpssBlock block = new TerminateBlock(new IntegerExpression(count));

        block.ShouldBeAssignableTo<GpssBlock>();
    }
}
