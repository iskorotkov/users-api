using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Admin;
using Auth.Hashing;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Context;
using Models.Entities;
using Models.Enums;
using Signup;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly WebApiContext _context;
        private readonly IMapper _mapper;
        private readonly AdminElevation _adminElevation;
        private readonly SignupThrottler _signupThrottler;
        private readonly PasswordHasher _passwordHasher;

        public UsersController(WebApiContext context, IMapper mapper, AdminElevation adminElevation,
            SignupThrottler signupThrottler, PasswordHasher passwordHasher)
        {
            _context = context;
            _mapper = mapper;
            _adminElevation = adminElevation;
            _signupThrottler = signupThrottler;
            _passwordHasher = passwordHasher;
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
            if (entity == null)
            {
                return NotFound();
            }

            entity.Login = user.Login;
            entity.GroupId = user.GroupId;

            _context.Entry(entity).State = EntityState.Modified;

            if (!await _adminElevation.CanEnterGroup(entity))
            {
                return BadRequest();
            }

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
            var activeState = await _context.UserStates.FirstOrDefaultAsync(s => s.Code == UserStateCode.Active);
            if (activeState == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            var hashed = _passwordHasher.Hash(user.Password);

            var entity = new User
            {
                Login = user.Login,
                PasswordHash = hashed.Hash,
                Salt = hashed.Salt,
                CreatedDate = DateTime.Now,
                GroupId = user.GroupId,
                StateId = activeState.Id
            };

            if (!await _adminElevation.CanEnterGroup(entity))
            {
                return BadRequest();
            }

            if (!await _signupThrottler.IsSignupAllowed(entity))
            {
                return Conflict();
            }

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = entity.Id }, _mapper.Map<UserGetDto>(entity));
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserGetDto>> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var blockedState = await _context.UserStates.FirstOrDefaultAsync(s => s.Code == UserStateCode.Blocked);
            if (blockedState == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            user.StateId = blockedState.Id;

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return _mapper.Map<UserGetDto>(user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
