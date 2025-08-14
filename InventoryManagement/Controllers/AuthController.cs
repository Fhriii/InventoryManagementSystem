using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly InventoryManagementContext _context;

    public AuthController(IConfiguration config, InventoryManagementContext context)
    {
        _config=config;
        _context = context;
    }
    
    [HttpPost("login")]
    public IActionResult login([FromBody]LoginDto login)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == login.name);
        if(user == null)
        {
            return Unauthorized("Usn Salah");
        }

        if (user.Password != login.password)
        {
            return Unauthorized("Password Salah");
        }
        
        var key =new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]{
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.Role,user.Role),
            new Claim(ClaimTypes.NameIdentifier,user.UserId.ToString())

        };

        var token = new JwtSecurityToken(

            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims : claims,
            expires:DateTime.UtcNow.AddHours(1),
            signingCredentials:creds
        );
        return Ok(new { token =new JwtSecurityTokenHandler().WriteToken(token)}) ;
    }
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var identity = User.Identity as ClaimsIdentity;
        if(identity == null)
        {
            return Unauthorized("User tidak ditemukan");
        }

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role= User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new
        {
            username = username,
            role = role
        });
    }
}
public record LoginDto(string name ,string password);
