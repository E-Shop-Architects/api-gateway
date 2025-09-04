using Microsoft.AspNetCore.Authentication.JwtBearer; // JWT
using Microsoft.IdentityModel.Tokens;                // Token param

var builder = WebApplication.CreateBuilder(args);

// CORS ekle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 🔐 AUTH: Keycloak ayarlarını ENV / appsettings’ten oku
var authority = Environment.GetEnvironmentVariable("AUTH__AUTHORITY")
                ?? builder.Configuration["Auth:Authority"]
                ?? "http://keycloak:8080/realms/main"; // docker içi varsayılan
var audience = Environment.GetEnvironmentVariable("AUTH__AUDIENCE")
                ?? builder.Configuration["Auth:Audience"]
                ?? "api-gateway"; // Gateway için ayrı clientId kullanabilirsin
var validateAudience = (Environment.GetEnvironmentVariable("AUTH__VALIDATEAUDIENCE") ?? "false").ToLower() == "true";

// 🔐 JWT doğrulama
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", o =>
    {
        o.Authority = authority;          // Keycloak issuer
        o.Audience = audience;           // Gateway clientId (veya kapatılabilir)
        o.RequireHttpsMetadata = false;   // dev/docker için
        o.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "role",
            NameClaimType = "email",
            ValidateAudience = validateAudience // genelde Gateway’de false; servislerde true
        };
    });

// 🔐 Policy (opsiyonel: istersen rol/claim şartı ekle)
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("GatewayAuth", p => p.RequireAuthenticatedUser()); // temel: giriş zorunlu
});

// ReverseProxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// CORS'u uygula
app.UseCors("AllowAll");

app.UseAuthentication(); 
app.UseAuthorization();

//app.MapReverseProxy().RequireAuthorization("GatewayAuth");
app.MapReverseProxy();

app.Run();
