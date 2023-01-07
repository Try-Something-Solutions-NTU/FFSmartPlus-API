using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Auth;
using Microsoft.AspNetCore.Authorization;

namespace FFsmartPlus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        /// <summary>
        /// User Login 
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(LoginRespDto), 200)]
        [ProducesResponseType( 401)]
        [Route("login")]
        public async Task<ActionResult<LoginRespDto>> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                return Ok(new
                    LoginRespDto(){
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
            }
            return Unauthorized();
        }
        /// <summary>
        /// Register user
        /// </summary>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }
        /// <summary>
        /// Admin function to add user to role
        /// </summary>
        [HttpPost]
        [Authorize(Roles = UserRoles.Admin)]
        [ProducesResponseType(typeof(Response), 200)]
        [ProducesResponseType( 401)]
        [ProducesResponseType(typeof(Response), 400)]
        [Route("{username}/Add-Role")]
        public async Task<IActionResult> AddRole(string username, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role does not exist!" });

            }
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User does not exist" });
            if( await _userManager.IsInRoleAsync(user, role))
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User Is already in role!" });
            await _userManager.AddToRoleAsync(user, role);
            return Ok(new Response { Status = "Success", Message = $"{username} added to {role} successfully!" });
        }
        /// <summary>
        /// Admin function to remove user from role
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Response), 200)]
        [ProducesResponseType( 401)]
        [ProducesResponseType(typeof(Response), 400)]
        [Authorize(Roles = UserRoles.Admin)]
        [Route("{username}/Remove-Role")]
        public async Task<IActionResult> RemoveRole(string username, string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "Role does not exist!" });
            }
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User does not exist" });
            if( !await _userManager.IsInRoleAsync(user, role))
                return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User Is not in  in role!" });
            await _userManager.RemoveFromRoleAsync(user, role);
            return Ok(new Response { Status = "Success", Message = $"{username} removed from {role} successfully!" });
        }
        /// <summary>
        /// DEV FUNCTION - Registers new user on all role
        /// </summary>
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User already exists!" });

            IdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User creation failed! Please check user details and try again." });

            if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Chef))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Chef));
            if (!await _roleManager.RoleExistsAsync(UserRoles.Delivery))
                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Delivery));

            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Admin);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Chef);
            }
            if (await _roleManager.RoleExistsAsync(UserRoles.Delivery))
            {
                await _userManager.AddToRoleAsync(user, UserRoles.Delivery);
            }
            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
