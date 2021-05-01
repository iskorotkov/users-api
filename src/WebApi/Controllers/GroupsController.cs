using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Context;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly WebApiContext _context;

        public GroupsController(WebApiContext context)
        {
            _context = context;
        }

        // GET: api/Groups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGroupGetDto>>> GetUserGroups()
        {
            return await _context.UserGroups
                .Select(group => new UserGroupGetDto
                {
                    Id = group.Id,
                    Code = group.Code,
                    Description = group.Description
                })
                .ToListAsync();
        }
    }
}
