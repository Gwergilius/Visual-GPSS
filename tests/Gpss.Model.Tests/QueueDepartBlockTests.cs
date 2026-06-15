using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class QueueDepartBlockTests
{
    [Theory, InlineData("Barber")]
    public void QueueBlock_QueueName_IsPreserved(string name)
    {
        new QueueBlock(new SymbolExpression(name))
            .QueueName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(name);
    }

    [Fact]
    public void QueueBlock_OptionalCount_DefaultsToNull()
    {
        new QueueBlock(new SymbolExpression("Q")).Count.ShouldBeNull();
    }

    [Theory, InlineData("Barber")]
    public void QueueBlock_IsAssignableToGpssBlock(string name)
    {
        ((GpssBlock)new QueueBlock(new SymbolExpression(name))).ShouldBeAssignableTo<GpssBlock>();
    }

    [Theory, InlineData("Barber")]
    public void DepartBlock_QueueName_IsPreserved(string name)
    {
        new DepartBlock(new SymbolExpression(name))
            .QueueName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(name);
    }

    [Fact]
    public void DepartBlock_OptionalCount_DefaultsToNull()
    {
        new DepartBlock(new SymbolExpression("Q")).Count.ShouldBeNull();
    }

    [Theory, InlineData("Barber")]
    public void DepartBlock_IsAssignableToGpssBlock(string name)
    {
        ((GpssBlock)new DepartBlock(new SymbolExpression(name))).ShouldBeAssignableTo<GpssBlock>();
    }
}
