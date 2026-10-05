using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Domain.Entities
{
    public class ClientTest
    {
        [Fact]
        public void Create_With_Valid_Data_Should_Succeed()
        {
            // Arrange & Act
            var client = new Client(ClientType.Individual, "John Doe", "ID123", "john@example.com", "123456789", "Some Address");

            // Assert
            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal(ClientType.Individual, client.Type);
            Assert.Equal("John Doe", client.Name);
            Assert.Equal("ID123", client.IdentificationNumber);
            Assert.Equal("john@example.com", client.Email);
            Assert.Equal("123456789", client.Phone);
            Assert.Equal("Some Address", client.Address);
        }

        [Theory]
        [InlineData(ClientType.Individual, "", "ID123")]
        [InlineData(ClientType.Individual, "John", "  ")]
        public void Constructor_Should_Throw_When_Required_Data_Is_Empty(
            ClientType clientType,
            string name,
            string identificationNumber)
        {
            // Act
            var exception = Record.Exception(() =>
                new Client(clientType, name, identificationNumber));

            // Assert
            Assert.IsType<ArgumentException>(exception);
        }

        [Fact]
        public void UpdateContactDetails_Should_Set_Values()
        {
            // Arrange
            var client = new Client(ClientType.Company, "Comp", "C123");

            // Act
            client.UpdateContactDetails("a@b.com", "555", "Addr");

            // Assert
            Assert.Equal("a@b.com", client.Email);
            Assert.Equal("555", client.Phone);
            Assert.Equal("Addr", client.Address);
        }

        [Fact]
        public void ChangeName_Should_Succeed()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "Name", "ID1");

            // Act
            client.ChangeName("John Doe");

            // Assert
            Assert.Equal("John Doe", client.Name);
        }

        [Fact]
        public void ChangeName_Should_Throw_When_Name_Empty()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "Name", "ID1");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => client.ChangeName(""));
        }

        [Fact]
        public void ChangeIdentificationNumber_Should_Succeed()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "Name", "ID1");

            // Act
            client.ChangeIdentificationNumber("ID123");

            // Assert
            Assert.Equal("ID123", client.IdentificationNumber);
        }

        [Fact]
        public void ChangeIdentificationNumber_Should_Throw_When_Empty()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "Name", "ID1");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => client.ChangeIdentificationNumber(""));
        }

        [Fact]
        public void ChangeType_Should_Update_Type()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "Name", "ID1");

            // Act
            client.ChangeType(ClientType.Company);

            // Assert
            Assert.Equal(ClientType.Company, client.Type);
        }
    }
}
