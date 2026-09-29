using Domain.Enums;

namespace Domain.Entities
{
    public class Building
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid CityId { get; private set; }
        public string Street { get; private set; }
        public string Number { get; private set; }
        public int ConstructionYear { get; private set; }
        public BuildingType Type { get; private set; }
        public int NumberOfFloors { get; private set; }
        public decimal SurfaceArea { get; private set; }
        public decimal InsuredValue { get; private set; }
        public bool IsFloodRiskZone { get; private set; }
        public bool IsEarthquakeRiskZone { get; private set; }
        private static void CheckClientId(Guid clientId)
        {
            if (clientId == Guid.Empty)
                throw new ArgumentException("Client is required.");
        }
        private static void CheckCityId(Guid cityId)
        {
            if (cityId == Guid.Empty)
                throw new ArgumentException("City is required.");
        }
        private static void CheckStreet(string street)
        {
            if (string.IsNullOrWhiteSpace(street))
                throw new ArgumentException("Street is required.");
        }
        private static void CheckStreetNr(string nr)
        {
            if (string.IsNullOrWhiteSpace(nr))
                throw new ArgumentException("Street is required.");
        }
        private static void CheckConstructionYear(int constructionYear)
        {
            if (constructionYear < 0)
                throw new ArgumentException(
                    "Number of construction year cannot be negative.");

            if (constructionYear > DateTime.UtcNow.Year)
            {
                throw new ArgumentException(
                    "Construction year cannot be in the future.");
            }
        }
        private static void CheckNrOfFloors(int numberOfFloors)
        {
            if (numberOfFloors < 0)
                throw new ArgumentException(
                    "Number of floors cannot be negative.");
        }
        private static void CheckSurfaceArea(decimal surfaceArea)
        {
            if (surfaceArea <= 0)
                throw new ArgumentException(
                    "Surface area must be greater than zero.");
        }
        private static void CheckInsuredValue(decimal insuredValue)
        {
            if (insuredValue <= 0)
                throw new ArgumentException(
                    "Insured value must be greater than zero.");
        }


        public Building(
            Guid clientId,
            Guid cityId,
            string street,
            string number,
            int constructionYear,
            BuildingType type,
            int numberOfFloors,
            decimal surfaceArea,
            decimal insuredValue,
            bool isFloodRiskZone = false,
            bool isEarthquakeRiskZone = false)
        {
            CheckClientId(clientId);
            CheckCityId(cityId);
            CheckStreet(street);
            CheckStreetNr(number);
            CheckConstructionYear(constructionYear);
            CheckNrOfFloors(numberOfFloors);
            CheckSurfaceArea(surfaceArea);
            CheckInsuredValue(insuredValue);

            Id = Guid.NewGuid();
            ClientId = clientId;
            CityId = cityId;
            Street = street;
            Number = number;
            ConstructionYear = constructionYear;
            Type = type;
            NumberOfFloors = numberOfFloors;
            SurfaceArea = surfaceArea;
            InsuredValue = insuredValue;
            IsFloodRiskZone = isFloodRiskZone;
            IsEarthquakeRiskZone = isEarthquakeRiskZone;
        }

        public void UpdateAddress(
            Guid cityId,
            string street,
            string number)
        {
            CheckCityId(cityId);
            CheckStreet(street);
            CheckStreetNr(number);

            CityId = cityId;
            Street = street;
            Number = number;
        }

        public void UpdateDetails(
            int constructionYear,
            BuildingType type,
            int numberOfFloors,
            decimal surfaceArea,
            decimal insuredValue)
        {
            CheckConstructionYear(constructionYear);
            CheckNrOfFloors(numberOfFloors);
            CheckSurfaceArea(surfaceArea);
            CheckInsuredValue(insuredValue);

            ConstructionYear = constructionYear;
            Type = type;
            NumberOfFloors = numberOfFloors;
            SurfaceArea = surfaceArea;
            InsuredValue = insuredValue;
        }

        public void UpdateRiskIndicators(
            bool isFloodRiskZone,
            bool isEarthquakeRiskZone)
        {
            IsFloodRiskZone = isFloodRiskZone;
            IsEarthquakeRiskZone = isEarthquakeRiskZone;
        }
    }
}
