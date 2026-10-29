using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestionOrdinateurs.Client;
using Blazored.SessionStorage; 
using Microsoft.AspNetCore.Components.Authorization;
using GestionOrdinateurs.Client.Auth;
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Tous les appels API du navigateur passent par la Gateway.
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("http://localhost:5010/") });

// On active le stockage de session
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped(sp => (JwtAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

await builder.Build().RunAsync();