using Insurance.Domain.Entities;

namespace Insurance.Domain.UnitTests.Entities
{
    public class CityTest
    {
        [Fact]
        public void Create_Should_Set_Properties()
        {
            // Arrange
            var countyId = Guid.NewGuid();

            // Act
            var city = new City(countyId, "Sample City", "12345");

            // Assert
            Assert.NotEqual(Guid.Empty, city.Id);
            Assert.Equal(countyId, city.CountyId);
            Assert.Equal("Sample City", city.Name);
            Assert.Equal("12345", city.PostalCode);
        }
        [Theory]
        [InlineData("CountyId")]
        [InlineData("Name")]
        [InlineData("PostalCode")]
        public void Create_Should_Throw_When_Required_Data_Is_Invalid(string invalidField)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                invalidField switch
                {
                    "CountyId" => new City(Guid.Empty, "Sample City", "12345"),
                    "Name" => new City(Guid.NewGuid(), "", "12345"),
                    "PostalCode" => new City(Guid.NewGuid(), "Sample City", ""),
                    _ => throw new ArgumentOutOfRangeException(nameof(invalidField))
                });
        }
    }
}
