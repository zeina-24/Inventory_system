using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ImsPosSystem.Application.DTOs.Auth;
using ImsPosSystem.Domain.Entities;

namespace ImsPosSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AccountController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDTO userDTO)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = userDTO.UserName,
                Email = userDTO.Email
            };

            var result = await _userManager.CreateAsync(user, userDTO.Password);
            if (result.Succeeded)
            {
                // Optionally add default role here. For now, new users don't have Admin permissions.
                return Ok(new { Message = "Account Creation Successful" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("AccountErrors", error.Description);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginUserDTO userDto)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(userDto.UserName);
            if (user != null)
            {
                bool found = await _userManager.CheckPasswordAsync(user, userDto.Password);
                if (found)
                {
                    string jti = Guid.NewGuid().ToString();
                    var roles = await _userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                        new Claim(JwtRegisteredClaimNames.Jti, jti)
                    };

                    if (roles != null)
                    {
                        foreach (var role in roles)
                        {
                            authClaims.Add(new Claim(ClaimTypes.Role, role));
                        }
                    }

                    var jwtKey = _config["JWT:Key"];
                    if (string.IsNullOrEmpty(jwtKey)) return StatusCode(500, "JWT Key missing.");

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                    var token = new JwtSecurityToken(
                        issuer: _config["JWT:Iss"],
                        audience: _config["JWT:Aud"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
            }
            ModelState.AddModelError("error", "Invalid Account / Password.");
        }
        return Unauthorized(ModelState);
    }
}
