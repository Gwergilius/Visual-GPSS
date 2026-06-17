using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Shouldly;

namespace Gpss.Model.Tests;

public sealed class SeizeReleaseBlockTests
{
    // -------------------------------------------------------------------------
    // SeizeBlock
    // -------------------------------------------------------------------------

    [Theory, InlineData("Barber")]
    public void SeizeBlock_FacilityName_IsPreserved(string facilityName)
    {
        var block = new SeizeBlock(new SymbolExpression(facilityName));

        block.FacilityName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(facilityName);
    }

    [Theory, InlineData("Server", "Checkout")]
    public void SeizeBlock_RecordEquality_DifferentNamesAreNotEqual(string nameA, string nameB)
    {
        nameA.ShouldNotBe(nameB);
        new SeizeBlock(new SymbolExpression(nameA)).ShouldNotBe(new SeizeBlock(new SymbolExpression(nameB)));
    }

    [Theory, InlineData("Server")]
    public void SeizeBlock_RecordEquality_SameNameIsEqual(string name)
    {
        new SeizeBlock(new SymbolExpression(name)).ShouldBe(new SeizeBlock(new SymbolExpression(name)));
    }

    [Theory, InlineData("Server")]
    public void SeizeBlock_WithLabel_LabelIsPreserved(string name)
    {
        var block = new SeizeBlock(new SymbolExpression(name)) { Label = "WAIT" };

        block.Label.ShouldBe("WAIT");
    }

    [Theory, InlineData("Server")]
    public void SeizeBlock_IsAssignableToGpssBlock(string name)
    {
        GpssBlock block = new SeizeBlock(new SymbolExpression(name));

        block.ShouldBeAssignableTo<GpssBlock>();
    }

    // -------------------------------------------------------------------------
    // ReleaseBlock
    // -------------------------------------------------------------------------

    [Theory, InlineData("Barber")]
    public void ReleaseBlock_FacilityName_IsPreserved(string facilityName)
    {
        var block = new ReleaseBlock(new SymbolExpression(facilityName));

        block.FacilityName.ShouldBeOfType<SymbolExpression>().Name.ShouldBe(facilityName);
    }

    [Theory, InlineData("Server")]
    public void ReleaseBlock_IsAssignableToGpssBlock(string name)
    {
        GpssBlock block = new ReleaseBlock(new SymbolExpression(name));

        block.ShouldBeAssignableTo<GpssBlock>();
    }

    // -------------------------------------------------------------------------
    // SymbolExpression
    // -------------------------------------------------------------------------

    [Theory, InlineData("Barber")]
    public void SymbolExpression_Name_IsPreserved(string name)
    {
        new SymbolExpression(name).Name.ShouldBe(name);
    }

    [Theory, InlineData("Barber", "Server")]
    public void SymbolExpression_RecordEquality_DifferentNamesAreNotEqual(string a, string b)
    {
        a.ShouldNotBe(b);
        new SymbolExpression(a).ShouldNotBe(new SymbolExpression(b));
    }
}
