using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly WebApiContext _context;
        private readonly IMapper _mapper;

        public UsersController(WebApiContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.Group)
                .Include(u => u.State)
                .Select(user => _mapper.Map<UserGetDto>(user))
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserGetDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Group)
                .Include(u => u.State)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return _mapper.Map<UserGetDto>(user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserPutDto user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            var entity = await _context.Users.FindAsync(user.Id);
            entity.Login = user.Login;
            entity.Password = user.Password;
            entity.GroupId = user.GroupId;
            entity.StateId = user.StateId;
            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserGetDto>> PostUser(UserPostDto user)
        {
            var entity = new User
            {
                Login = user.Login,
                Password = user.Password,
                CreatedDate = DateTime.Now,
                GroupId = user.GroupId,
                StateId = user.StateId
            };

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = entity.Id }, _mapper.Map<UserGetDto>(entity));
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
