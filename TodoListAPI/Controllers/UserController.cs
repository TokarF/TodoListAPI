using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoListAPI.Models;
using TodoListAPI.Services;
using TodoListAPI.ViewModels;

namespace TodoListAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly TodoListContext context;
        private readonly IConfiguration configuration;

        public UserController(TodoListContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }


        [HttpGet(Name = "Get All Users")]
        public async Task<ActionResult<List<UserModel>>> GetUsers()
        {
            return Ok(await context.Users.ToListAsync());
        }


        /// <summary>
        /// Returns the user by a specified ID
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<List<UserModel>>> GetUserById(int userId)
        {
            return Ok(await context.Users.FindAsync(userId));
        }



        /// <summary>
        /// Add a user
        /// </summary>
        /// <response code="201">Returns the newly created user</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        public async Task<ActionResult<UserModel>> RegisterUser(RegisterViewModel registerViewModel)
        {
            UserModel user = new UserModel();
            user.Email = registerViewModel.Email;
            user.Username = registerViewModel.Username;
            user.Password = BCrypt.Net.BCrypt.HashPassword(registerViewModel.Password);
            user.Age = registerViewModel.Age;

            if (await context.Users.SingleOrDefaultAsync(x => x.Username == user.Username) is not null)
            {
                ModelState.AddModelError("Username", "The username is already taken");
                return BadRequest("The username has been already taken. Please select another one!");

            }

            context.Users.Add(user);
            await context.SaveChangesAsync();

            //return Ok(await context.Users.FindAsync(user.UserId));
            return CreatedAtAction("GetUserById", new { userId = user.UserId }, user);

        }


        /// <summary>
        /// Add a user
        /// </summary>
        /// <response code="201">Returns the newly created user</response>
        /// <response code="400">If the item is null</response>
        [HttpPost]
        public async Task<ActionResult<UserModel>> Login(LoginViewModel loginViewModel)
        {
            UserModel user = await context.Users.SingleOrDefaultAsync(x => x.Username == loginViewModel.Username);

            //string passwordHash = BCrypt.Net.BCrypt.HashPassword(loginViewModel.Password);

            bool pass = BCrypt.Net.BCrypt.Verify(loginViewModel.Password, user.Password);

            if (user is null || !BCrypt.Net.BCrypt.Verify(loginViewModel.Password, user.Password))
                return BadRequest("Invalid user credentials!");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),

            };

            var token = new JwtSecurityToken
            (
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                notBefore: DateTime.UtcNow,
                signingCredentials: new SigningCredentials
                (
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
                    SecurityAlgorithms.HmacSha256
                )
                
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(tokenString);



        }

    }
}
