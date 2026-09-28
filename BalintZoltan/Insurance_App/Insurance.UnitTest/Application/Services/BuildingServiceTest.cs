using Application.DTO.Buildings;
using Application.DTO.Common;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Application.Helper;
using Xunit;

namespace Application.Services
{
    public class BuildingServiceTest
    {
        private readonly BuildingService _service;
        private readonly FakeRepositories _fakeRepositories;

        public BuildingServiceTest()
        {
            _fakeRepositories = new FakeRepositories();
            _service = new BuildingService(_fakeRepositories.Building, _fakeRepositories.Client, _fakeRepositories.Geography);            
        }

        [Fact]
        public async Task CreateAsync_Should_Create_When_Client_And_City_Exist()
        {
            var existingClient = new Domain.Entities.Client(ClientType.Individual, "John", "ID1");
            _fakeRepositories.Client.Seed(existingClient);

            var cityId = Guid.NewGuid();
            _fakeRepositories.Geography.SeedCity(cityId);

            var request = new CreateBuildingRequest
            {
                ClientId = existingClient.Id,
                CityId = cityId,
                Street = "Main",
                Number = "1",
                ConstructionYear = 2000,
                Type = BuildingType.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100m,
                InsuredValue = 10000m,
                IsFloodRiskZone = false,
                IsEarthquakeRiskZone = true
            };

            var dto = await _service.CreateBuildingAsync(request);

            Assert.NotNull(dto);
            Assert.Equal(request.ClientId, dto.ClientId);
            Assert.Equal(request.CityId, dto.CityId);
            Assert.Equal(request.Street, dto.Street);
            Assert.Equal(request.Number, dto.Number);
            Assert.Equal(request.SurfaceArea, dto.SurfaceArea);
            Assert.Equal(request.IsEarthquakeRiskZone, dto.IsEarthquakeRiskZone);
            // repository should have stored one building
            Assert.Single(_fakeRepositories.Building.Storage);
        }

        [Fact]
        public async Task CreateBuildingAsync_Should_Throw_When_Client_Not_Found()
        {
            var request = new CreateBuildingRequest
            {
                ClientId = Guid.NewGuid(),
                CityId = Guid.NewGuid(),
                Street = "Main",
                Number = "1",
                ConstructionYear = 2000,
                Type = BuildingType.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100m,
                InsuredValue = 10000m,
                IsFloodRiskZone = false,
                IsEarthquakeRiskZone = true
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateBuildingAsync(request));
            Assert.Equal("Client was not found.", ex.Message);
        }

        [Fact]
        public async Task CreateBuildingAsync_Should_Throw_When_City_Not_Found()
        {
            var existingClient = new Domain.Entities.Client(ClientType.Individual, "John", "ID1");
            _fakeRepositories.Client.Seed(existingClient);

            var request = new CreateBuildingRequest
            {
                ClientId = existingClient.Id,
                CityId = Guid.NewGuid(),
                Street = "Main",
                Number = "1",
                ConstructionYear = 2000,
                Type = BuildingType.Residential,
                NumberOfFloors = 2,
                SurfaceArea = 100m,
                InsuredValue = 10000m,
                IsFloodRiskZone = false,
                IsEarthquakeRiskZone = true
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateBuildingAsync(request));
            Assert.Equal("City was not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateBuildingAsync_Should_Update_When_Building_Exists()
        {
            var client = new Domain.Entities.Client(ClientType.Individual, "John", "1234567890123");
            _fakeRepositories.Client.Seed(client);
            var clientId = client.Id;
            var cityId = Guid.NewGuid();
            var building = new Building(clientId, cityId, "OldSt", "1", 1990, BuildingType.Administrative, 1, 50m, 1000m);
            _fakeRepositories.Building.Storage.Add(building);
            _fakeRepositories.Geography.SeedCity(cityId);

            var update = new UpdateBuildingRequest
            {
                CityId = cityId,
                Street = "NewSt",
                Number = "2",
                ConstructionYear = 2001,
                Type = BuildingType.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 200m,
                InsuredValue = 5000m,
                IsFloodRiskZone = true,
                IsEarthquakeRiskZone = false
            };

            var dto = await _service.UpdateBuildingAsync(building.Id, update);

            Assert.Equal(building.Id, dto.Id);
            Assert.Equal(update.Street, dto.Street);
            Assert.Equal(update.Number, dto.Number);
            Assert.Equal(update.NumberOfFloors, dto.NumberOfFloors);
            Assert.Equal(update.SurfaceArea, dto.SurfaceArea);
            Assert.Equal(update.IsFloodRiskZone, dto.IsFloodRiskZone);
        }

        [Fact]
        public async Task UpdateBuildingAsync_Should_Throw_When_Building_Not_Found()
        {
            var update = new UpdateBuildingRequest
            {
                CityId = Guid.NewGuid(),
                Street = "NewSt",
                Number = "2",
                ConstructionYear = 2001,
                Type = BuildingType.Residential,
                NumberOfFloors = 3,
                SurfaceArea = 200m,
                InsuredValue = 5000m,
                IsFloodRiskZone = true,
                IsEarthquakeRiskZone = false
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateBuildingAsync(Guid.NewGuid(), update));
            Assert.Equal("Building was not found.", ex.Message);
        }

        [Fact]
        public async Task GetBuildingByIdAsync_Should_Return_Dto_When_Building_Exists()
        {
            var clientId = Guid.NewGuid();
            var cityId = Guid.NewGuid();
            var building = new Building(clientId, cityId, "St", "1", 1995, BuildingType.Residential, 2, 80m, 2000m);
            _fakeRepositories.Building.Storage.Add(building);

            var dto = await _service.GetBuildingByIdAsync(building.Id);

            Assert.NotNull(dto);
            Assert.Equal(building.Id, dto!.Id);
            Assert.Equal(building.Street, dto.Street);
        }

        [Fact]
        public async Task GetBuildingByIdAsync_Should_Return_Null_When_Not_Found()
        {
            var dto = await _service.GetBuildingByIdAsync(Guid.NewGuid());

            Assert.Null(dto);
        }

        [Fact]
        public async Task GetBuildingByClientIdAsync_Should_Return_Buildings_For_Client()
        {
            var client = new Domain.Entities.Client(ClientType.Individual, "John", "1234567890123");
            _fakeRepositories.Client.Seed(client);
            var clientId = client.Id;
            var cityId = Guid.NewGuid();
            var b1 = new Building(clientId, cityId, "A", "1", 1990, BuildingType.Residential, 1, 50m, 1000m);
            var b2 = new Building(clientId, cityId, "B", "2", 1991, BuildingType.Residential, 2, 75m, 1500m);
            _fakeRepositories.Building.Storage.Add(b1);
            _fakeRepositories.Building.Storage.Add(b2);

            var list = await _service.GetBuildingByClientIdAsync(
                clientId,
                new PaginationRequest { PageSize = 10 });

            Assert.Equal(2, list.TotalCount);
            Assert.Equal(2, list.Items.Count);
            Assert.Contains(list.Items, x => x.Id == b1.Id);
            Assert.Contains(list.Items, x => x.Id == b2.Id);
        }

        [Fact]
        public async Task GetBuildingByClientIdAsync_Should_Return_Empty_When_None()
        {
            var client = new Domain.Entities.Client(ClientType.Individual, "John", "1234567890123");
            _fakeRepositories.Client.Seed(client);

            var list = await _service.GetBuildingByClientIdAsync(
                client.Id,
                new PaginationRequest());

            Assert.NotNull(list);
            Assert.Empty(list.Items);
            Assert.Equal(0, list.TotalCount);
        }
    }
}
