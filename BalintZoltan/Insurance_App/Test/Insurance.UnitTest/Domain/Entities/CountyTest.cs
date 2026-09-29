using Domain.Entities;

namespace Domain.UnitTests.Entities
{
    public class CountyTest
    {
        [Fact]
        public void Create_Should_Set_Properties_And_Empty_Cities()
        {
            var countryId = Guid.NewGuid();
            var county = new County(countryId, "Some County");

            Assert.NotEqual(Guid.Empty, county.Id);
            Assert.Equal(countryId, county.CountryId);
            Assert.Equal("Some County", county.Name);
            Assert.NotNull(county.Cities);
            Assert.Empty(county.Cities);
        }
        [Fact]
        public void Create_Should_Throw_When_PostalCode_Empty()
        {
            var countryId = Guid.Empty;
            Assert.Throws<ArgumentException>(() => new County(countryId, "Some County"));
        }
        [Fact]
        public void Create_Should_Throw_When_Name_Empty()
        {
            var countryId = Guid.NewGuid();
            Assert.Throws<ArgumentException>(() => new County(countryId, ""));
        }
    }
}
