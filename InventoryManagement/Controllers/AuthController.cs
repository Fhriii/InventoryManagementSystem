using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApplication1.Dto;
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

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> register([FromBody]Register dto)
    {
        if (dto.role != "Admin" && dto.role != "Staff")
        {   
            return BadRequest("Role must Admin or Staff");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.username);
        if (user != null)
        {
            return BadRequest("Username is used");
        }
        var newUser = new User
        {
            Username = dto.username,
            FullName = dto.fullname,
            Password = dto.password,
            IsActive = true,
            Role = dto.role,
            CreatedAt = DateTime.Now
        };
        try
        {
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                Username = newUser.Username,
                FullName = newUser.FullName,
                Role = newUser.Role,
                Password = newUser.Password
            });
        }
        catch(Exception e){
            return BadRequest(e.Message);
        }
    }
}
public record LoginDto(string name ,string password);
