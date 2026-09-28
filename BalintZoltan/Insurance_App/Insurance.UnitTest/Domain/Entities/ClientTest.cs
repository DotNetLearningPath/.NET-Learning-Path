using System;
using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace Domain.UnitTests.Entities
{
    public class ClientTest
    {
        [Fact]
        public void Create_With_Valid_Data_Should_Succeed()
        {
            var client = new Client(ClientType.Individual, "John Doe", "ID123", "john@example.com", "123456789", "Some Address");

            Assert.NotEqual(Guid.Empty, client.Id);
            Assert.Equal(ClientType.Individual, client.Type);
            Assert.Equal("John Doe", client.Name);
            Assert.Equal("ID123", client.IdentificationNumber);
            Assert.Equal("john@example.com", client.Email);
            Assert.Equal("123456789", client.Phone);
            Assert.Equal("Some Address", client.Address);
        }

        [Theory]
        [InlineData("Name")]
        [InlineData("IdentificationNumber")]
        public void Constructor_Should_Throw_When_Required_Data_Is_Empty(string invalidField)
        {
            Assert.Throws<ArgumentException>(() =>
                invalidField switch
                {
                    "Name" => new Client(ClientType.Individual, "", "ID123"),
                    "IdentificationNumber" => new Client(ClientType.Individual, "John", "  "),
                    _ => throw new ArgumentOutOfRangeException(nameof(invalidField))
                });
        }

        [Fact]
        public void UpdateContactDetails_Should_Set_Values()
        {
            var client = new Client(ClientType.Company, "Comp", "C123");

            client.UpdateContactDetails("a@b.com", "555", "Addr");

            Assert.Equal("a@b.com", client.Email);
            Assert.Equal("555", client.Phone);
            Assert.Equal("Addr", client.Address);
        }

        [Fact]
        public void ChangeName_Should_Succeed()
        {
            var client = new Client(ClientType.Individual, "Name", "ID1");
            client.ChangeName("John Doe");
            Assert.Equal("John Doe", client.Name);
        }

        [Fact]
        public void ChangeName_Should_Throw_When_Name_Empty()
        {
            var client = new Client(ClientType.Individual, "Name", "ID1");
            Assert.Throws<ArgumentException>(() => client.ChangeName(""));
        }

        [Fact]
        public void ChangeIdentificationNumber_Should_Succeed()
        {
            var client = new Client(ClientType.Individual, "Name", "ID1");
            client.ChangeIdentificationNumber("ID123");
            Assert.Equal("ID123", client.IdentificationNumber);
        }

        [Fact]
        public void ChangeIdentificationNumber_Should_Throw_When_Empty()
        {
            var client = new Client(ClientType.Individual, "Name", "ID1");
            Assert.Throws<ArgumentException>(() => client.ChangeIdentificationNumber(""));
        }

        [Fact]
        public void ChangeType_Should_Update_Type()
        {
            var client = new Client(ClientType.Individual, "Name", "ID1");
            client.ChangeType(ClientType.Company);
            Assert.Equal(ClientType.Company, client.Type);
        }
    }
}
