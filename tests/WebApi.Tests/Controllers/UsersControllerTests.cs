using System;
using System.Linq;
using System.Threading.Tasks;
using Admin;
using Auth.Hashing;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models.Context;
using Models.Entities;
using Models.Enums;
using Shouldly;
using Signup;
using Utils.Seeding;
using WebApi.Controllers;
using WebApi.DTOs;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public abstract class UsersControllerTests<T> where T : ISeeder, new()
    {
        private readonly IMapper _mapper;
        private readonly ISeeder _seeder;

        private readonly UserGroup _adminGroup;
        private readonly UserGroup _userGroup;

        private readonly UserState _activeState;
        private readonly UserState _blockedState;

        protected UsersControllerTests()
        {
            _seeder = new T();

            var host = Program.CreateHostBuilder(new string[] { }).Build();
            _mapper = host.Services.GetRequiredService<IMapper>();

            using var context = new WebApiContext(_seeder.DbContextOptions);

            _adminGroup = context.UserGroups.First(g => g.Code == UserGroupCode.Admin);
            _userGroup = context.UserGroups.First(g => g.Code == UserGroupCode.User);

            _activeState = context.UserStates.First(s => s.Code == UserStateCode.Active);
            _blockedState = context.UserStates.First(s => s.Code == UserStateCode.Blocked);
        }

        private UsersController CreateController(WebApiContext context)
        {
            var adminElevation = new AdminElevation(context);
            var signupThrottler = new SignupThrottler(context, TimeSpan.FromMinutes(10));
            var hasher = new PasswordHasher();
            var controller = new UsersController(context, _mapper, adminElevation, signupThrottler, hasher);
            return controller;
        }

        [Fact]
        public async Task GetAllUsers()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var getResult = await controller.GetUsers();
            getResult.Result.ShouldBeAssignableTo<OkResult>();

            var users = getResult.Value.ToList();
            var mapped = context.Users.AsEnumerable().Select(u => _mapper.Map<UserGetDto>(u));

            users.Count().ShouldBe(10);
            users.ToList().ShouldBeEquivalentTo(mapped.ToList());
        }

        [Fact]
        public async Task GetSpecificExistingUser()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var id = context.Users.OrderBy(u => u.Login).First().Id;

            var getResult = await controller.GetUser(id);
            getResult.Result.ShouldBeAssignableTo<OkResult>();

            var user = getResult.Value;
            var mapped = _mapper.Map<UserGetDto>(await context.Users.FindAsync(id));

            user.Id.ShouldBe(id);
            user.ShouldBeEquivalentTo(mapped);
        }

        [Fact]
        public async Task GetSpecificNonExistingUser()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var maxId = await context.Users.MaxAsync(u => u.Id);

            var getResult = await controller.GetUser(maxId + 1);
            getResult.Result.ShouldBeAssignableTo<NotFoundResult>();

            getResult = await controller.GetUser(-1);
            getResult.Result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task AddAndDeleteUserInUserGroup()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var userToAdd = new UserPostDto
            {
                Login = "new login",
                Password = "new password",
                GroupId = _userGroup.Id
            };

            var postResult = await controller.PostUser(userToAdd);
            postResult.Result.ShouldBeAssignableTo<CreatedAtActionResult>();

            var createdUser = (UserGetDto) ((CreatedAtActionResult) postResult.Result).Value;
            createdUser.Login.ShouldBe(userToAdd.Login);
            createdUser.Group.ShouldBeEquivalentTo(_mapper.Map<UserGroupGetDto>(_userGroup));
            createdUser.State.ShouldBeEquivalentTo(_mapper.Map<UserStateGetDto>(_activeState));
            createdUser.CreatedDate.ShouldBeInRange(DateTime.Now - TimeSpan.FromSeconds(5), DateTime.Now);

            (await context.Users.FindAsync(createdUser.Id)).ShouldNotBeNull();
            context.Users.Count().ShouldBe(11);

            var deleteResult = await controller.DeleteUser(createdUser.Id);
            deleteResult.Result.ShouldBeAssignableTo<OkResult>();

            var deletedUsed = deleteResult.Value;
            deletedUsed.Login.ShouldBe(userToAdd.Login);
            deletedUsed.Group.ShouldBeEquivalentTo(_mapper.Map<UserGroupGetDto>(_userGroup));
            deletedUsed.State.ShouldBeEquivalentTo(_mapper.Map<UserStateGetDto>(_blockedState));
            deletedUsed.CreatedDate.ShouldBe(createdUser.CreatedDate);

            (await context.Users.FindAsync(createdUser.Id)).ShouldNotBeNull();
            context.Users.Count().ShouldBe(11);

            var user = await context.Users.FindAsync(createdUser.Id);
            context.Users.Remove(user);
            await context.SaveChangesAsync();

            context.Users.Count().ShouldBe(10);
        }

        [Fact]
        public async Task AddUserToUserGroupAndModifyUsingMatchingId()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var userToAdd = new UserPostDto
            {
                Login = "new login",
                Password = "new password",
                GroupId = _userGroup.Id
            };

            var postResult = await controller.PostUser(userToAdd);
            postResult.Result.ShouldBeAssignableTo<CreatedAtActionResult>();

            var createdUser = (UserGetDto) ((CreatedAtActionResult) postResult.Result).Value;

            var changes = new UserPutDto
            {
                Id = createdUser.Id,
                Login = "changed login",
                GroupId = _userGroup.Id,
            };

            var putResult = await controller.PutUser(createdUser.Id, changes);
            putResult.ShouldBeAssignableTo<NoContentResult>();

            var getResult = await controller.GetUser(createdUser.Id);
            getResult.Result.ShouldBeAssignableTo<OkResult>();

            var changedUser = getResult.Value;

            changedUser.Id.ShouldBe(createdUser.Id);
            changedUser.Login.ShouldBe(changes.Login);
            changedUser.State.ShouldBeEquivalentTo(_mapper.Map<UserStateGetDto>(_activeState));
            changedUser.Group.ShouldBeEquivalentTo(_mapper.Map<UserGroupGetDto>(_userGroup));
            changedUser.CreatedDate.ShouldBe(createdUser.CreatedDate);

            var user = await context.Users.FindAsync(createdUser.Id);
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddUserToUserGroupAndModifyUsingNotMatchingId()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var userToAdd = new UserPostDto
            {
                Login = "new login",
                Password = "new password",
                GroupId = _userGroup.Id
            };

            var postResult = await controller.PostUser(userToAdd);
            postResult.Result.ShouldBeAssignableTo<CreatedAtActionResult>();

            var createdUser = (UserGetDto) ((CreatedAtActionResult) postResult.Result).Value;

            var changes = new UserPutDto
            {
                Id = createdUser.Id,
                Login = "changed login",
                GroupId = _userGroup.Id,
            };

            var putResult = await controller.PutUser(createdUser.Id + 1, changes);
            putResult.ShouldBeAssignableTo<BadRequestResult>();

            var user = await context.Users.FindAsync(createdUser.Id);
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task PutNonExistingUser()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var maxId = await context.Users.MaxAsync(u => u.Id);

            var result = await controller.PutUser(maxId + 1, new UserPutDto
            {
                Id = maxId + 1,
                Login = "",
                GroupId = 0
            });
            result.ShouldBeAssignableTo<NotFoundResult>();

            result = await controller.PutUser(-1, new UserPutDto
            {
                Id = -1,
                Login = "",
                GroupId = 0
            });
            result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteNonExistingUser()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var maxId = await context.Users.MaxAsync(u => u.Id);

            var result = await controller.DeleteUser(maxId + 1);
            result.Result.ShouldBeAssignableTo<NotFoundResult>();

            result = await controller.DeleteUser(-1);
            result.Result.ShouldBeAssignableTo<NotFoundResult>();
        }

        [Fact]
        public async Task AddAdminWhenNoAdminPresent()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            await foreach (var user in context.Users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            var userToAdd = new UserPostDto
            {
                Login = "login",
                Password = "password",
                GroupId = _adminGroup.Id
            };

            var result = await controller.PostUser(userToAdd);
            result.Result.ShouldBeAssignableTo<CreatedAtActionResult>();
            var addedUser = (UserGetDto) ((CreatedAtActionResult) result.Result).Value;

            (await context.Users.FindAsync(addedUser.Id)).ShouldNotBeNull();
        }

        [Fact]
        public async Task AddAdminWhenAdminPresentFails()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var users = context.Users.ToList();
            foreach (var user in users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            users[new Random().Next(users.Count)].GroupId = _adminGroup.Id;

            await context.SaveChangesAsync();

            var userToAdd = new UserPostDto
            {
                Login = "login",
                Password = "password",
                GroupId = _adminGroup.Id
            };

            var result = await controller.PostUser(userToAdd);
            result.Result.ShouldBeAssignableTo<BadRequestResult>();

            context.Users.Count().ShouldBe(10);
        }

        [Fact]
        public async Task ChangeUserToAdminWhenNoAdminPresent()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            await foreach (var user in context.Users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            var userToChange = await context.Users.FirstAsync();

            var changes = new UserPutDto
            {
                Id = userToChange.Id,
                Login = "some new login",
                GroupId = _adminGroup.Id,
            };

            var result = await controller.PutUser(userToChange.Id, changes);
            result.ShouldBeAssignableTo<NoContentResult>();

            var changedUser = await context.Users.FindAsync(userToChange.Id);
            changedUser.Login.ShouldBe(changes.Login);
            changedUser.GroupId.ShouldBe(changes.GroupId);
        }

        [Fact]
        public async Task ChangeUserToAdminWhenAdminPresentFails()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var users = context.Users.ToList();
            foreach (var user in users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            users[new Random().Next(users.Count)].GroupId = _adminGroup.Id;

            await context.SaveChangesAsync();

            var userToChange = await context.Users.FirstAsync(u => u.Group.Code != UserGroupCode.Admin);
            var oldData = new { userToChange.Login, userToChange.GroupId };

            var changes = new UserPutDto
            {
                Id = userToChange.Id,
                Login = "some new login",
                GroupId = _adminGroup.Id,
            };

            var result = await controller.PutUser(userToChange.Id, changes);
            result.ShouldBeAssignableTo<BadRequestResult>();

            userToChange.Login.ShouldBe(oldData.Login);
            userToChange.GroupId.ShouldBe(oldData.GroupId);
        }

        [Fact]
        public async Task ChangeAdminUserToAdminGroup()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var users = context.Users.ToList();
            foreach (var user in users)
            {
                user.GroupId = _userGroup.Id;
                context.Entry(user).State = EntityState.Modified;
            }

            var admin = users[new Random().Next(users.Count)];
            admin.GroupId = _adminGroup.Id;

            await context.SaveChangesAsync();

            var changes = new UserPutDto
            {
                Id = admin.Id,
                Login = "some new login",
                GroupId = _adminGroup.Id,
            };

            var result = await controller.PutUser(admin.Id, changes);
            result.ShouldBeAssignableTo<NoContentResult>();

            var changedUser = await context.Users.FindAsync(admin.Id);
            changedUser.Login.ShouldBe(changes.Login);
            changedUser.GroupId.ShouldBe(changes.GroupId);
        }

        [Fact]
        public async Task AddUserWithSameLoginImmediately()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var result = await controller.PostUser(new UserPostDto
            {
                Login = "login",
                Password = "password",
                GroupId = _userGroup.Id
            });
            var createdUser = (UserGetDto) ((CreatedAtActionResult) result.Result).Value;

            result = await controller.PostUser(new UserPostDto
            {
                Login = "login",
                Password = "password2",
                GroupId = _userGroup.Id
            });
            result.Result.ShouldBeAssignableTo<ConflictResult>();

            context.Users.Count().ShouldBe(11);

            context.Users.Remove(await context.Users.FindAsync(createdUser.Id));
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddUserWithSameLoginAfterTime()
        {
            await using var context = new WebApiContext(_seeder.DbContextOptions);
            var controller = CreateController(context);

            var result = await controller.PostUser(new UserPostDto
            {
                Login = "login",
                Password = "password",
                GroupId = _userGroup.Id
            });
            var createdUser1 = (UserGetDto) ((CreatedAtActionResult) result.Result).Value;

            await foreach (var user in context.Users)
            {
                user.CreatedDate = DateTime.Now.AddHours(-1);
                context.Entry(user).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            result = await controller.PostUser(new UserPostDto
            {
                Login = "login",
                Password = "password2",
                GroupId = _userGroup.Id
            });
            result.Result.ShouldBeAssignableTo<CreatedAtActionResult>();
            var createdUser2 = (UserGetDto) ((CreatedAtActionResult) result.Result).Value;

            context.Users.Count().ShouldBe(12);

            var users = await context.Users.Where(u => u.Id == createdUser1.Id || u.Id == createdUser2.Id)
                .ToListAsync();
            context.Users.RemoveRange(users);
            await context.SaveChangesAsync();
        }
    }

    public class SqliteUsersControllerTests : UsersControllerTests<SqliteSeeder>
    {
    }

    public class SqlServerUsersControllerTests : UsersControllerTests<SqlServerSeeder>
    {
    }
}
