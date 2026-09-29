using Domain.Entities;
using Domain.Enums;

namespace Domain.UnitTests.Entities
{
    public class BuildingTest
    {
        [Fact]
        public void Create_With_Valid_Data_Should_Succeed()
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();

            var building = new Building(
                clientId,
                cityId,
                "Main St",
                "10A",
                2000,
                BuildingType.Residential,
                3,
                120.5m,
                250000m,
                isFloodRiskZone: true,
                isEarthquakeRiskZone: false);

            Assert.NotEqual(Guid.Empty, building.Id);
            Assert.Equal(clientId, building.ClientId);
            Assert.Equal(cityId, building.CityId);
            Assert.Equal("Main St", building.Street);
            Assert.Equal("10A", building.Number);
            Assert.Equal(2000, building.ConstructionYear);
            Assert.Equal(BuildingType.Residential, building.Type);
            Assert.Equal(3, building.NumberOfFloors);
            Assert.Equal(120.5m, building.SurfaceArea);
            Assert.Equal(250000m, building.InsuredValue);
            Assert.True(building.IsFloodRiskZone);
            Assert.False(building.IsEarthquakeRiskZone);
        }

        [Theory]
        [InlineData("ClientId")]
        [InlineData("CityId")]
        [InlineData("Street")]
        [InlineData("SurfaceArea")]
        public void Constructor_Should_Throw_When_Required_Data_Is_Invalid(string invalidField)
        {
            Assert.Throws<ArgumentException>(() =>
                invalidField switch
                {
                    "ClientId" => new Building(Guid.Empty, Guid.NewGuid(), "St", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m),
                    "CityId" => new Building(Guid.NewGuid(), Guid.Empty, "St", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m),
                    "Street" => new Building(Guid.NewGuid(), Guid.NewGuid(), "", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m),
                    "SurfaceArea" => new Building(Guid.NewGuid(), Guid.NewGuid(), "St", "1", 1990, BuildingType.Administrative, 1, 0m, 1000m),
                    _ => throw new ArgumentOutOfRangeException(nameof(invalidField))
                });
        }

        [Fact]
        public void UpdateAddress_Should_Succeed()
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var building = new Building(clientId, cityId, "St", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m);

            building.UpdateAddress(cityId, "New St", "2");
            Assert.Equal(cityId, building.CityId);
            Assert.Equal("New St", building.Street);
            Assert.Equal("2", building.Number);
        }

        [Theory]
        [InlineData("CityId")]
        [InlineData("Street")]
        [InlineData("Number")]
        public void UpdateAddress_Should_Throw_When_Data_Is_Invalid(string invalidField)
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var building = new Building(clientId, cityId, "St", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m);

            Assert.Throws<ArgumentException>(() =>
            {
                switch (invalidField)
                {
                    case "CityId":
                        building.UpdateAddress(Guid.Empty, "New St", "2");
                        break;
                    case "Street":
                        building.UpdateAddress(cityId, "", "2");
                        break;
                    case "Number":
                        building.UpdateAddress(cityId, "New St", "");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(invalidField));
                }
            });
        }

        [Fact]
        public void UpdateDetails_Should_Succeed()
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var building = new Building(clientId, cityId, "St", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m);

            building.UpdateDetails(2000, BuildingType.Residential, 3, 120.5m, 250000m);

            Assert.Equal(2000, building.ConstructionYear);
            Assert.Equal(BuildingType.Residential, building.Type);
            Assert.Equal(3, building.NumberOfFloors);
            Assert.Equal(120.5m, building.SurfaceArea);
            Assert.Equal(250000m, building.InsuredValue);
        }

        [Theory]
        [InlineData("InsuredValue")]
        [InlineData("SurfaceArea")]
        [InlineData("NumberOfFloors")]
        public void UpdateDetails_Should_Throw_When_Value_Is_NonPositive(string invalidField)
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var building = new Building(clientId, cityId, "St", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m);

            Assert.Throws<ArgumentException>(() =>
            {
                switch (invalidField)
                {
                    case "InsuredValue":
                        building.UpdateDetails(1991, BuildingType.Residential, 2, 100m, 0m);
                        break;
                    case "SurfaceArea":
                        building.UpdateDetails(1991, BuildingType.Residential, 2, 0m, 1100m);
                        break;
                    case "NumberOfFloors":
                        building.UpdateDetails(1991, BuildingType.Residential, -1, 50m, 1100m);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(invalidField));
                }
            });
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void UpdateRiskIndicators_Should_Update_Only_Risk_Flags(
            bool isFloodRiskZone,
            bool isEarthquakeRiskZone)
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var building = new Building(
                clientId,
                cityId,
                "St",
                "1",
                1990,
                BuildingType.Administrative,
                2,
                75m,
                1500m,
                isFloodRiskZone: false,
                isEarthquakeRiskZone: false);

            var beforeFloors = building.NumberOfFloors;
            var beforeSurface = building.SurfaceArea;
            var beforeInsured = building.InsuredValue;

            building.UpdateRiskIndicators(isFloodRiskZone, isEarthquakeRiskZone);

            Assert.Equal(isFloodRiskZone, building.IsFloodRiskZone);
            Assert.Equal(isEarthquakeRiskZone, building.IsEarthquakeRiskZone);
            Assert.Equal(beforeFloors, building.NumberOfFloors);
            Assert.Equal(beforeSurface, building.SurfaceArea);
            Assert.Equal(beforeInsured, building.InsuredValue);
        }
    }
}
