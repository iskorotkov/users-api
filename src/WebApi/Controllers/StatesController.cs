using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Db.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatesController : ControllerBase
    {
        private readonly WebApiContext _context;
        private readonly IMapper _mapper;

        public StatesController(WebApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/States
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserStateGetDto>>> GetUserStates()
        {
            return await _context.UserStates
                .Select(state => _mapper.Map<UserStateGetDto>(state))
                .ToListAsync();
        }
    }
}
