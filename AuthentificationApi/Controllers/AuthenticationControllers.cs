using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using GestionOrdinateurs.Shared;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthenticationController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return BadRequest("Le login est obligatoire.");
        }

        var client = _httpClientFactory.CreateClient("GestionOrdinateursClient");
        using var response = await client.GetAsync($"/api/users/{Uri.EscapeDataString(login.Trim())}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Unauthorized("Utilisateur introuvable.");
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "L'API principale n'a pas pu vérifier le login.");
        }

        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        if (user is null || string.IsNullOrWhiteSpace(user.Login) || !Enum.IsDefined(user.Role))
        {
            return StatusCode(StatusCodes.Status502BadGateway, "L'API principale a renvoyé un utilisateur invalide.");
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("La clé JWT (Jwt:Key) n'est pas configurée.");
        }

        var key = Encoding.UTF8.GetBytes(jwtKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { Token = tokenHandler.WriteToken(token) });
    }
}

public class UserDto
{
    public string Login { get; set; } = string.Empty;
    public RoleEnum Role { get; set; }
}