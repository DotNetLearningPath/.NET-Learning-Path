using System;
using Domain.Entities;
using Xunit;

namespace Domain.UnitTests.Entities
{
    public class CountryTest
    {
        [Fact]
        public void Create_Should_Set_Properties_And_Empty_Counties()
        {
            var country = new Country("Testland");

            Assert.NotEqual(Guid.Empty, country.Id);
            Assert.Equal("Testland", country.Name);
            Assert.NotNull(country.Counties);
            Assert.Empty(country.Counties);
        }
        [Fact]
        public void Create_Should_Throw_When_Name_Empty()
        {
            Assert.Throws<ArgumentException>(() => new Country(""));
        }
    }
}
