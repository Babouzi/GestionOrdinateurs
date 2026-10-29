using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace GestionOrdinateurs.Client.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ISessionStorageService _sessionStorage;

    public JwtAuthenticationStateProvider(ISessionStorageService sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _sessionStorage.GetItemAsync<string>("jwtToken");

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }

    public void NotifyUserAuthentication(string token)
    {
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public void NotifyUserLogout()
    {
        var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
        if (keyValuePairs is null)
        {
            return [];
        }

        var claims = keyValuePairs
            .Where(kvp => kvp.Key != "realm_access")
            .Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()))
            .ToList();

        if (keyValuePairs.TryGetValue("realm_access", out var realmAccess)
            && realmAccess.ValueKind == JsonValueKind.Object
            && realmAccess.TryGetProperty("roles", out var roles)
            && roles.ValueKind == JsonValueKind.Array)
        {
            claims.AddRange(roles.EnumerateArray()
                .Where(role => role.ValueKind == JsonValueKind.String)
                .Select(role => new Claim(ClaimTypes.Role, role.GetString()!)));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}