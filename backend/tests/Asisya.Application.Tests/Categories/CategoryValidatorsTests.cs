using Asisya.Application.Features.Categories.Commands.DeleteCategory;
using Asisya.Application.Features.Categories.Commands.UpdateCategory;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Categories;

public class CategoryValidatorsTests
{
    private readonly UpdateCategoryCommandValidator _updateValidator = new();
    private readonly DeleteCategoryCommandValidator _deleteValidator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateValidator_ShouldFail_WhenCategoryIdIsInvalid(int invalidId)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = invalidId,
            CategoryName = "Valid Name"
        };

        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateValidator_ShouldFail_WhenCategoryNameIsEmpty(string emptyName)
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = 1,
            CategoryName = emptyName
        };

        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
    }

    [Fact]
    public void UpdateValidator_ShouldFail_WhenCategoryNameExceeds50Characters()
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = 1,
            CategoryName = new string('A', 51)
        };

        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryName");
    }

    [Fact]
    public void UpdateValidator_ShouldPass_WhenAllFieldsAreValid()
    {
        var command = new UpdateCategoryCommand
        {
            CategoryId = 1,
            CategoryName = "SERVIDORES",
            Description = "Standard enterprise servers"
        };

        var result = _updateValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void DeleteValidator_ShouldFail_WhenCategoryIdIsInvalid(int invalidId)
    {
        var command = new DeleteCategoryCommand(invalidId);

        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public void DeleteValidator_ShouldPass_WhenCategoryIdIsValid()
    {
        var command = new DeleteCategoryCommand(1);

        var result = _deleteValidator.Validate(command);
        result.IsValid.Should().BeTrue();
    }
}
