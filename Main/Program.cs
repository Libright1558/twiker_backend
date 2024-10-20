using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using twiker_backend.Swagger.SwaggerAttributes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using StackExchange.Redis;
using twiker_backend.Models.DatabaseContext;
using twiker_backend.Redis;
using twiker_backend.Db.Repository;
using twiker_backend.ServiceLayer;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.TraversePath().Load();

// Add configuration source
builder.Configuration.AddEnvironmentVariables();

// Add Logger
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IAccessTokenService, AccessTokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRedisPostData, PostInfo>();
builder.Services.AddScoped<IRedisUserData, UserInfo>();
builder.Services.AddScoped<IDbUserInfo, DbUserInfo>();
builder.Services.AddScoped<IDbPostInfo, DbPostInfo>();
var redis = ConnectionMultiplexer.Connect(DotNetEnv.Env.GetString("RedisConnection"));
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddDbContext<TwikerContext>(options => options.UseNpgsql(DotNetEnv.Env.GetString("connection_string")));

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer("AccessScheme", async options =>
{
    var publicKeyPath = DotNetEnv.Env.GetString("public_key");
    var publicKey = await File.ReadAllTextAsync(publicKeyPath!);
    var _pubRsa = RSA.Create();
    _pubRsa.ImportFromPem(publicKey.ToCharArray());
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new RsaSecurityKey(_pubRsa),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var userIdClaim = context.Principal?.FindFirst("userId")?.Value;
            var usernameClaim = context.Principal?.FindFirst("username")?.Value;
            var accessTokenClaim = context.Principal?.FindFirst("Access_token")?.Value;

            if (userIdClaim == null || usernameClaim == null || accessTokenClaim == null)
            {
                context.Fail("Required claims are missing.");
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Fail("Token Expired!");
            }
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer("JwtScheme", async options =>
{
    var publicKeyPath = DotNetEnv.Env.GetString("public_key");
    var publicKey = await File.ReadAllTextAsync(publicKeyPath!);
    var _pubRsa = RSA.Create();
    _pubRsa.ImportFromPem(publicKey.ToCharArray());
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new RsaSecurityKey(_pubRsa),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var userIdClaim = context.Principal?.FindFirst("userId")?.Value;
            var usernameClaim = context.Principal?.FindFirst("username")?.Value;
            var accessTokenClaim = context.Principal?.FindFirst("Authentication_token")?.Value;

            if (userIdClaim == null || usernameClaim == null || accessTokenClaim == null)
            {
                context.Fail("Required claims are missing.");
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Fail("Token Expired!");
            }
            return Task.CompletedTask;
        }
    };
});

// Add authorization
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Twiker API", Version = "v1" });

    // Define the JwtScheme security scheme
    c.AddSecurityDefinition("JwtScheme", new OpenApiSecurityScheme
    {
        Description = "JWT_Token Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Define the AccessScheme security scheme
    c.AddSecurityDefinition("AccessScheme", new OpenApiSecurityScheme
    {
        Description = "Access_Token Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Add global security requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "JwtScheme" }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "AccessScheme" }
            },
            new string[] {}
        }
    });

    c.OperationFilter<SwaggerAuthorizeCheckOperationFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
