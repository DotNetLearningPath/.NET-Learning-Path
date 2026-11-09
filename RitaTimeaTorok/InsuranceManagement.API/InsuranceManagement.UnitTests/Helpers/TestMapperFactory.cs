using AutoMapper;
using InsuranceManagement.Application.Mapping;
using Microsoft.Extensions.Logging.Abstractions;

namespace InsuranceManagement.UnitTests.Helpers
{
    internal static class TestMapperFactory
    {
        public static IMapper Create()
        {
            var configuration = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<GeographyMappingProfile>();
                    cfg.AddProfile<ClientMappingProfile>();
                    cfg.AddProfile<BuildingMappingProfile>();
                },
                NullLoggerFactory.Instance);

            return configuration.CreateMapper();
        }
    }
}