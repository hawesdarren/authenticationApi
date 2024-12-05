using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Pkix;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
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
    //options.AddPolicy("Email", policy => policy.RequireClaim("Email"));
    options.AddPolicy("Email", policy => policy.RequireClaim(ClaimTypes.Email));
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
app.MapControllers();

app.Run();
