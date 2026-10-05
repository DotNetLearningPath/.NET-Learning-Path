using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Domain.Entities
{
    public class CountyTest
    {
        [Fact]
        public void Create_Should_Set_Properties_And_Empty_Cities()
        {
            // Arrange
            var countryId = Guid.NewGuid();

            // Act
            var county = new County(countryId, "Some County");

            // Assert
            Assert.NotEqual(Guid.Empty, county.Id);
            Assert.Equal(countryId, county.CountryId);
            Assert.Equal("Some County", county.Name);
            Assert.NotNull(county.Cities);
            Assert.Empty(county.Cities);
        }
        [Fact]
        public void Create_Should_Throw_When_PostalCode_Empty()
        {
            // Act
            var countryId = Guid.Empty;

            // Assert
            Assert.Throws<ArgumentException>(() => new County(countryId, "Some County"));
        }
        [Fact]
        public void Create_Should_Throw_When_Name_Empty()
        {
            // Act
            var countryId = Guid.NewGuid();

            // Assert
            Assert.Throws<ArgumentException>(() => new County(countryId, ""));
        }
    }
}
