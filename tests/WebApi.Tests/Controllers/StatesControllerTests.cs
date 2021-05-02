using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Models.Context;
using Models.Enums;
using Shouldly;
using Utils.Seeding;
using WebApi.Controllers;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public abstract class StatesControllerTests<T> where T : ISeeder, new()
    {
        private readonly ISeeder _seeder;
        private readonly IMapper _mapper;

        protected StatesControllerTests()
        {
            _seeder = new T();

            var host = Program.CreateHostBuilder(new string[] { }).Build();
            _mapper = host.Services.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task GetAllGroups()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = new StatesController(context, _mapper);

            var states = await controller.GetUserStates();

            states.Value.Count().ShouldBe(2);
            states.Value.Any(g => g.Code == UserStateCode.Active).ShouldBe(true);
            states.Value.Any(g => g.Code == UserStateCode.Blocked).ShouldBe(true);
        }
    }

    public class SqliteStatesControllerTests : StatesControllerTests<SqliteSeeder>
    {
    }

    public class SqlServerStatesControllerTests : StatesControllerTests<SqlServerSeeder>
    {
    }
}
