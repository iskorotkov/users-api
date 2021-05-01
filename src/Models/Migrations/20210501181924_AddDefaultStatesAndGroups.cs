using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Models.Entities;
using Models.Enums;

namespace Models.Migrations
{
    public partial class AddDefaultStatesAndGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("user_states",  new[]{ "Id", "Code", "Description" }, 
                new object[] { 1, (int)UserStateCode.Active, "Active user" });
            migrationBuilder.InsertData("user_states", new[] { "Id", "Code", "Description" }, 
                new object[] { 2, (int) UserStateCode.Blocked, "Blocked user" });

            migrationBuilder.InsertData("user_groups", new[] { "Id", "Code", "Description" },
                new object[] { 1, (int) UserGroupCode.Admin, "Admin group" });
            migrationBuilder.InsertData("user_groups", new[] { "Id", "Code", "Description" },
                new object[] { 2, (int) UserGroupCode.User, "User group" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("user_states", "Id", new [] { new { Id = 1 } , new { Id = 2 } });
            migrationBuilder.DeleteData("user_groups", "Id", new[] { new { Id = 1 }, new { Id = 2 } });
        }
    }
}
