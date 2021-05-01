using System.Reflection;
using AutoMapper;

namespace WebApi.Mapper
{
    public static class MapperExtensions
    {
        public static void Configure(IMapperConfigurationExpression config)
        {
            config.AddMaps(Assembly.GetExecutingAssembly());

            config.SourceMemberNamingConvention = new PascalCaseNamingConvention();
            config.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        }
    }
}
