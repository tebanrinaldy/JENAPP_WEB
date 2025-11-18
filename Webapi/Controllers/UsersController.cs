using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Data;
using Webapi.Models;
using Webapi.Repositories;
using Webapi.Services;
using BCrypt.Net;
using Webapi;
using Microsoft.AspNetCore.Authorization;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly Userservice _userservice;
        private readonly JwtTokensGenerator _jwtTokensGenerator;

        public UsersController(Userservice userservice, JwtTokensGenerator jwtTokensGenerator)
        {
            _userservice = userservice;
            _jwtTokensGenerator = jwtTokensGenerator;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _userservice.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _userservice.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);

        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }
            if (!string.IsNullOrEmpty(user.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            await _userservice.UpdateUserAsync(user);

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {

            await _userservice.RegisterUser(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userservice.GetUserByIdAsync(id);
            if (user == null)
                 return NotFound();

            await _userservice.DeleteUserAsync(id);
            return NoContent();
        }
        // 🟣 POST: api/users/Login 
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _userservice.ValidateUser(login.Username, login.Password);

            if (user == null)
                return Unauthorized("Usuario o contraseña incorrectos");

            var token = _jwtTokensGenerator.GenerateToken(user.Username);

            return Ok(new
            {
                message = "Inicio de sesión exitoso",
                user = new { user.Id, user.Username },
                token   
            });
        }


        public class LoginDto
        {
            public string Username { get; set; }
            public string Password { get; set; } 
        }

    }
}
