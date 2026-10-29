var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration du HttpClientFactory 
builder.Services.AddHttpClient("GestionOrdinateursClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiUrls:GestionOrdinateursApi"]!);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();