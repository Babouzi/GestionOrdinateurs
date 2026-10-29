using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

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
    public async Task<IActionResult> Login([FromBody] LoginRequest credentials)
    {
        if (string.IsNullOrWhiteSpace(credentials.Username) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            return BadRequest("Le nom d'utilisateur et le mot de passe sont obligatoires.");
        }

        var tokenEndpoint = _configuration["Keycloak:TokenEndpoint"]
            ?? "http://localhost:8180/realms/GestionOrdinateurs/protocol/openid-connect/token";
        var clientId = _configuration["Keycloak:ClientId"] ?? "gestion-ordinateurs-client";
        using var client = _httpClientFactory.CreateClient();
        using var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["grant_type"] = "password",
            ["username"] = credentials.Username,
            ["password"] = credentials.Password
        });

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync(tokenEndpoint, requestContent);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, "Keycloak est indisponible.");
        }

        using (response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
                || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return Unauthorized("Identifiants Keycloak invalides.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(StatusCodes.Status502BadGateway, "Keycloak n'a pas pu émettre de jeton.");
            }

            var token = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                return StatusCode(StatusCodes.Status502BadGateway, "La réponse Keycloak ne contient pas de jeton d'accès.");
            }

            return Ok(token);
        }
    }

    /*
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] string login)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return BadRequest("Le login est obligatoire.");
        }

        var client = _httpClientFactory.CreateClient("GestionOrdinateursClient");
        using var response = await client.GetAsync($"/api/users/{Uri.EscapeDataString(login.Trim())}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("La clé JWT (Jwt:Key) n'est pas configurée.");
        }

        var key = System.Text.Encoding.UTF8.GetBytes(jwtKey);
        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.Login),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { Token = tokenHandler.WriteToken(token) });
    }
    */
}

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
}