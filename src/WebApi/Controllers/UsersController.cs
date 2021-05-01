using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Context;
using WebApi.DTOs;
using WebApi.Entities;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly WebApiContext _context;

        public UsersController(WebApiContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserGetDto>>> GetUsers()
        {
            return await _context.Users
                .Select(user => new UserGetDto
                {
                    Id = user.Id,
                    Login = user.Login,
                    CreatedDate = user.CreatedDate,
                    Group = new UserGroupGetDto
                    {
                        Id = user.Group.Id,
                        Code = user.Group.Code,
                        Description = user.Group.Description
                    },
                    State = new UserStateGetDto
                    {
                        Id = user.State.Id,
                        Code = user.State.Code,
                        Description = user.State.Description
                    }
                })
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

            return new UserGetDto
            {
                Id = user.Id,
                Login = user.Login,
                CreatedDate = user.CreatedDate,
                Group = new UserGroupGetDto
                {
                    Id = user.Group.Id,
                    Code = user.Group.Code,
                    Description = user.Group.Description
                },
                State = new UserStateGetDto
                {
                    Id = user.State.Id,
                    Code = user.State.Code,
                    Description = user.State.Description
                }
            };
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
        public async Task<ActionResult<User>> PostUser(UserPostDto user)
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

            return CreatedAtAction("GetUser", new { id = entity.Id }, new UserGetDto
            {
                Id = entity.Id,
                Login = entity.Login,
                CreatedDate = entity.CreatedDate,
                Group = new UserGroupGetDto
                {
                    Id = entity.Group.Id,
                    Code = entity.Group.Code,
                    Description = entity.Group.Description
                },
                State = new UserStateGetDto
                {
                    Id = entity.State.Id,
                    Code = entity.State.Code,
                    Description = entity.State.Description
                }
            });
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
