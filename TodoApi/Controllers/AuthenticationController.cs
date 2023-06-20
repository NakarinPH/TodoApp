using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TodoApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthenticationController(IConfiguration config)
	{
        _config = config;
    }

    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int Id, string FirstName, string LastName, string UserName);

    [HttpPost("token")]
    [AllowAnonymous]

    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        var user = ValidateCredentials(data);
        if (user is null) 
        {
            return Unauthorized();
        }

        string token = GenerateToken(user);

        return Ok(token);
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                _config.GetValue<string>("Authentication:SecretKey"))); 
        
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claim = new();
        claim.Add(new(JwtRegisteredClaimNames.Sub,user.Id.ToString()));
        claim.Add(new(JwtRegisteredClaimNames.UniqueName, user.UserName));
        claim.Add(new(JwtRegisteredClaimNames.GivenName, user.FirstName));
        claim.Add(new(JwtRegisteredClaimNames.FamilyName, user.LastName));

        var token = new JwtSecurityToken(
            _config.GetValue<String>("Authentication:Issuer"),
            _config.GetValue<String>("Authentication:Audience"),
            claim,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private UserData? ValidateCredentials(AuthenticationData data)
    {
        // REPLACE THIS WITH A CALL TO YOUR AUTH SYSTEM
        if (CompareValues(data.UserName,"admin") &&
            CompareValues(data.Password,"admin"))
        {
            return new UserData(1,"Admin", "P", data.UserName!);
        }

        if (CompareValues(data.UserName, "nakarin") &&
            CompareValues(data.Password, "1234"))
        {
            return new UserData(2, "Nakarin", "Philangam", data.UserName!);
        }

        return null;
    }

    private bool CompareValues(string? actual, string? expected) 
    {
        if (actual is not null)
        {
            if (actual.Equals(expected)) 
            {
                return true;
            }
        }

        return false;
    }
}
