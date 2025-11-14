using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration.Json;
using System.Security.Claims;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var secretFiles = Directory.EnumerateFiles(".", "secrets.json", SearchOption.AllDirectories);
foreach (var path in secretFiles) { 
    builder.Configuration.AddJsonFile(path);
}
//builder.Configuration.AddJsonFile(secretFiles.ToString(), optional: true);
var issuerSigningKey = builder.Configuration["authentication:issuerSigningKey"];
var validIssuer = builder.Configuration["authentication:validIssuer"];
var validAudience = builder.Configuration["authentication:validAudience"];

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = validIssuer,
            ValidateAudience = true,
            ValidAudience = validAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authentication", policy => policy.RequireClaim(ClaimTypes.Authentication));
    options.AddPolicy("Email", policy => policy.RequireClaim(ClaimTypes.Email));
});
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("https://hawes.co.nz", "https://192.168.1.137:443", "https://192.168.164.129:443")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
// Important Authorization must be below Authentication
app.UseAuthorization();
app.UseExceptionHandler("/Error");
//Enable CORS
app.UseCors("CorsPolicy");
app.MapControllers();
app.Run();
