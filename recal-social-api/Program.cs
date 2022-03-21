using System.Collections.Immutable;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using recal_social_api.Interfaces;
using recal_social_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.WebHost.UseKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5000, listenOptions =>
    {
        listenOptions.UseHttps(new X509Certificate2("certificate.pfx", "Passord01"));
    });
});

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", b =>
{
    b.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();



app.UseHsts();
app.UseCors("CorsPolicy");
// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();