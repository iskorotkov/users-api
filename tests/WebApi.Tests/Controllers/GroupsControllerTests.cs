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
    public abstract class GroupsControllerTests<T> where T : ISeeder, new()
    {
        private readonly ISeeder _seeder;
        private readonly IMapper _mapper;

        protected GroupsControllerTests()
        {
            _seeder = new T();

            var host = Program.CreateHostBuilder(new string[] { }).Build();
            _mapper = host.Services.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task GetAllGroups()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = new GroupsController(context, _mapper);

            var groups = await controller.GetUserGroups();

            groups.Value.Count().ShouldBe(2);
            groups.Value.Any(g => g.Code == UserGroupCode.Admin).ShouldBe(true);
            groups.Value.Any(g => g.Code == UserGroupCode.User).ShouldBe(true);
        }
    }

    public class SqliteGroupsControllerTests : GroupsControllerTests<SqliteSeeder>
    {
    }

    public class SqlServerGroupsControllerTests : GroupsControllerTests<SqlServerSeeder>
    {
    }
}
