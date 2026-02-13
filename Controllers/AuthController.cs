using GrawiaaApp.API.Models;
using GrawiaaApp.API.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GrawiaaApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Register a new parent account
        /// </summary>
        [HttpPost("register/parent")]
        public async Task<IActionResult> RegisterParent([FromBody] RegisterParentDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest(new { message = "This email is already registered." });

            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                Role = "Parent",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Parent account created successfully." });
        }

        /// <summary>
        /// Register a new child account with age verification
        /// </summary>
        [HttpPost("register/child")]
        public async Task<IActionResult> RegisterChild([FromBody] RegisterChildDto model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest(new { message = "This username is already taken. Please choose another one." });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            var user = new ApplicationUser
            {
                UserName = model.Username,
                FullName = model.FullName,
                Age = model.Age,
                CharacterName = model.CharacterName,
                CharacterType = model.CharacterType,
                Role = "Child",
                TrustScore = 50,
                CreatedAt = DateTime.UtcNow,
                ParentId = (currentUserRole == "Parent") ? int.Parse(currentUserId!) : null
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = "Child account created successfully! Get ready for your first adventure 🚀", isLinked = user.ParentId != null });
        }

        /// <summary>
        /// Login for parent or child and get JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UsernameOrEmail)
                       ?? await _userManager.FindByEmailAsync(model.UsernameOrEmail);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("FullName", user.FullName)
                };

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                return Ok(new AuthResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    Role = user.Role,
                    FullName = user.FullName,
                    UserId = user.Id,
                    PasswordResetRequired = user.PasswordResetRequired
                });
            }

            return Unauthorized(new { message = "Invalid login credentials. Please check your username/email and password." });
        }
    }
}
