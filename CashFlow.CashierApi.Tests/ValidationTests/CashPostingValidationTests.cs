using CashFlow.CashierApi.Validations;
using CashFlow.Domain.Models;
using FluentValidation.TestHelper;

namespace CashFlow.CashierApi.Tests.ValidationTests
{
    public class CashPostingValidationTests
    {
        private readonly CashPostingRequestValidator _validator;

        public CashPostingValidationTests()
        {
            _validator = new CashPostingRequestValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Zero()
        {
            // Arrange
            var model = new CashPostingRequest { Amount = 0, PostingType = "C", Description = "Test" };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Have_Error_When_Amount_Is_Negative()
        {
            // Arrange
            var model = new CashPostingRequest { Amount = -100, PostingType = "C", Description = "Test" };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Amount_Is_Positive()
        {
            // Arrange
            var model = new CashPostingRequest { Amount = 100, PostingType = "C", Description = "Test" };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Fact]
        public void Should_Have_Error_When_PostingType_Is_Invalid()
        {
            // Arrange
            var model = new CashPostingRequest { Amount = 100, PostingType = "X", Description = "Test" };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.PostingType);
        }

        [Theory]
        [InlineData("C")]
        [InlineData("D")]
        public void Should_Not_Have_Error_When_PostingType_Is_Valid(string postingType)
        {
            // Arrange
            var model = new CashPostingRequest { Amount = 100, PostingType = postingType, Description = "Test" };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.PostingType);
        }

        [Fact]
        public void Should_Have_Error_When_Description_Exceeds_Maximum_Length()
        {
            // Arrange
            var model = new CashPostingRequest
            {
                Amount = 100,
                PostingType = "C",
                Description = new string('A', 256) // 256 characters exceeds the 255 limit
            };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Description_Is_Valid()
        {
            // Arrange
            var model = new CashPostingRequest
            {
                Amount = 100,
                PostingType = "C",
                Description = new string('A', 255) // Exactly 255 characters
            };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Should_Be_Valid_When_Model_Is_Complete_And_Valid()
        {
            // Arrange
            var model = new CashPostingRequest
            {
                Amount = 100,
                PostingType = "C",
                Description = "Valid description"
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
