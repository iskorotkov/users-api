using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Admin;
using Auth.Hashing;
using AutoMapper;
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
                .Where(u => u.State.Code == UserStateCode.Active)
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
            if (user == null || user.State.Code != UserStateCode.Active)
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

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var entity = await _context.Users
                    .Include(u => u.State)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);
                if (entity == null || entity.State.Code != UserStateCode.Active)
                {
                    return NotFound();
                }

                if (await _context.UserGroups.FindAsync(user.GroupId) == null)
                {
                    return BadRequest();
                }

                if (!await _adminElevation.CanEnterGroup(user.GroupId, user.Id))
                {
                    return BadRequest();
                }

                entity.Login = user.Login;
                entity.GroupId = user.GroupId;

                _context.Entry(entity).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }

                return Conflict();
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserGetDto>> PostUser(UserPostDto user)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                if (await _context.UserGroups.FindAsync(user.GroupId) == null)
                {
                    return BadRequest();
                }

                if (!await _adminElevation.CanEnterGroup(user.GroupId))
                {
                    return BadRequest();
                }

                if (!await _signupThrottler.IsSignupAllowed(user.Login))
                {
                    return Conflict();
                }

                var activeState = await _context.UserStates.FirstAsync(s => s.Code == UserStateCode.Active);
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

                _context.Users.Add(entity);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return CreatedAtAction("GetUser", new { id = entity.Id }, _mapper.Map<UserGetDto>(entity));
            }
            catch (DBConcurrencyException e)
            {
                Console.WriteLine(e);
                return Conflict();
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<UserGetDto>> DeleteUser(int id)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var user = await _context.Users
                    .Include(u => u.State)
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (user == null || user.State.Code != UserStateCode.Active)
                {
                    return NotFound();
                }

                var blockedState = await _context.UserStates.FirstAsync(s => s.Code == UserStateCode.Blocked);
                user.StateId = blockedState.Id;

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return _mapper.Map<UserGetDto>(user);
            }
            catch (DBConcurrencyException e)
            {
                Console.WriteLine(e);
                return Conflict();
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
