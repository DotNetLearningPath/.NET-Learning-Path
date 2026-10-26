using Insurance.Application.Validators;
using Insurance.Domain.Enums;
using Insurance.Tests.Helpers;

namespace Insurance.Tests.Unit
{
    public class ClientValidatorTests
    {
        private readonly ClientValidator _validator = new();

        [Fact]
        public void Validate_WhenClientIsValid_DoesNotThrow()
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();

            // Act
            var exception = Record.Exception(() => _validator.Validate(client));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_WhenClientIsNull_ThrowsArgumentNullException()
        {
            // Act
            var exception = Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));

            // Assert
            Assert.Equal("client", exception.ParamName);
        }

        [Fact]
        public void Validate_WhenClientTypeIsInvalid_ThrowsArgumentException()
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();
            client.ClientType = (ClientType)123111241;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(client));

            // Assert
            Assert.Contains("Invalid client type", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WhenNameIsInvalid_ThrowsArgumentException(string? name)
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();
            client.Name = name!;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(client));

            // Assert
            Assert.Contains("Name is required", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WhenIdentificationNumberIsInvalid_ThrowsArgumentException(
            string? identificationNumber)
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();
            client.IdentificationNumber = identificationNumber!;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(client));

            // Assert
            Assert.Contains("Identification number is required", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WhenEmailIsMissing_ThrowsArgumentException(string? email)
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();
            client.Email = email!;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(client));

            // Assert
            Assert.Contains("Email is required", exception.Message);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("nicu@")]
        public void Validate_WhenEmailFormatIsInvalid_ThrowsArgumentException(
            string email)
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();
            client.Email = email;

            // Act
            var exception = Assert.Throws<ArgumentException>(
                () => _validator.Validate(client));

            // Assert
            Assert.Contains("Invalid email format", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WhenPhoneIsMissing_ThrowsArgumentException(string? phone)
        {
            // Arrange
            var client = ClientTestDataHelper.CreateValidClient();
            client.Phone = phone!;

            // Act
            var exception = Assert.Throws<ArgumentException>(() => _validator.Validate(client));

            // Assert
            Assert.Contains("Phone number is required", exception.Message);
        }
    }
}