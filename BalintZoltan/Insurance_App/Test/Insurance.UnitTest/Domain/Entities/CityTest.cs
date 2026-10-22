using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Domain.Entities
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
        [InlineData("CountyId", "Sample City", "12345")]
        [InlineData(null, "", "12345")]
        [InlineData(null, "Sample City", "")]
        public void Create_Should_Throw_When_Required_Data_Is_Invalid(
            string? countyId,
            string name,
            string postalCode)
        {
            // Arrange
            var parsedCountyId = countyId is null
                ? Guid.NewGuid()
                : Guid.Empty;

            // Act
            var exception = Record.Exception(() =>
                new City(parsedCountyId, name, postalCode));

            // Assert
            Assert.IsType<ArgumentException>(exception);
        }
    }
}
