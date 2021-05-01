using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly WebApiContext _context;
        private readonly IMapper _mapper;

        public GroupsController(WebApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Groups
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGroupGetDto>>> GetUserGroups()
        {
            return await _context.UserGroups
                .Select(group => _mapper.Map<UserGroupGetDto>(group))
                .ToListAsync();
        }
    }
}
