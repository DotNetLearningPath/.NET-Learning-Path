using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Domain.Entities
{
    public class CountryTest
    {
        [Fact]
        public void Create_Should_Set_Properties()
        {
            // Arrange & Act
            var country = new Country("Testland");

            // Assert
            Assert.NotEqual(Guid.Empty, country.Id);
            Assert.Equal("Testland", country.Name);
        }
        [Fact]
        public void Create_Should_Throw_When_Name_Empty()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Country(""));
        }
    }
}
