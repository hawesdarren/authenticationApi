using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration.Json;
using System.Security.Claims;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
//var secretFiles = Directory.EnumerateFiles(".", "secrets.json", SearchOption.AllDirectories);
/*foreach (var path in secretFiles) { 
    builder.Configuration.AddJsonFile(path);
}*/
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                     .AddJsonFile("secrets.json", optional: true, reloadOnChange: false)
                     .AddEnvironmentVariables();


// Bind Options with validation and fail on start if invalid
builder.Services
    .AddOptions<AuthenticationOptions>()
    .Bind(builder.Configuration.GetSection("authentication"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.IssuerSigningKey), "authentication.issuerSigningKey is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ValidIssuer), "authentication.validIssuer is required")
    .Validate(o => !string.IsNullOrWhiteSpace(o.ValidAudience), "authentication.validAudience is required")
    .ValidateOnStart();

builder.Services
    .AddOptions<SmtpOptions>()
    .Bind(builder.Configuration.GetSection("smtp2go"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "smtp2go.apiKey is required")
    .ValidateOnStart();



builder.Services.Configure<AuthenticationOptions>(builder.Configuration.GetSection("authentication"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("smtp2go"));

//builder.Configuration.AddJsonFile(secretFiles.ToString(), optional: true);
//var issuerSigningKey = builder.Configuration["authentication:issuerSigningKey"];
//var validIssuer = builder.Configuration["authentication:validIssuer"];
//var validAudience = builder.Configuration["authentication:validAudience"];

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
var authenticationOptions = builder.Configuration.GetSection("authentication").Get<AuthenticationOptions>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authenticationOptions.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = authenticationOptions.ValidAudience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationOptions.IssuerSigningKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authentication", policy => policy.RequireClaim(ClaimTypes.Authentication));
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
// Important Authorization must be after Authentication
app.UseAuthorization();
app.UseExceptionHandler("/Error");
app.MapControllers();
app.Run();


// --- Options classes ---
public sealed class AuthenticationOptions
{
    public string IssuerSigningKey { get; set; } = "";
    public string ValidIssuer { get; set; } = "";
    public string ValidAudience { get; set; } = "";
    public string DatabaseConnectionString { get; set; } = "";
}
public sealed class SmtpOptions
{
    public string ApiKey { get; set; } = "";
}

