using AutoMapper;
using WebApi.Mapper;
using Xunit;

namespace WebApi.Tests
{
    public class MapperTests
    {
        [Fact]
        public void CheckMapperConfiguration()
        {
            var config = new MapperConfiguration(MapperExtensions.Configure);
            config.AssertConfigurationIsValid();
        }
    }
}
