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
    public class StatesController : ControllerBase
    {
        private readonly WebApiContext _context;

        public StatesController(WebApiContext context)
        {
            _context = context;
        }

        // GET: api/States
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserStateGetDto>>> GetUserStates()
        {
            return await _context.UserStates
                .Select(state => new UserStateGetDto
                {
                    Id = state.Id,
                    Code = state.Code,
                    Description = state.Description
                })
                .ToListAsync();
        }
    }
}
