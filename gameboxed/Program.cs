using Application.Leyer.Interfaces;
using Infrastructure.Leyer.MyDbSetting; // Contains AppDbContext
using Infrastructure.Leyer.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Configure EF Core with SQL Server ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Register specific repositories ---
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IGameRatingRepository, GameRatingRepository>();
builder.Services.AddScoped<ITokenService, TokenService>(); // Add the TokenService

// --- Add controllers ---
builder.Services.AddControllers();

// --- Configure Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyGameApp API", Version = "v1" });
    // Optional: Add JWT bearer support in Swagger if needed.

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});


// --- Configure JWT Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero // Remove delay of token expiration
    };

    // Add custom token validation
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            var token = context.SecurityToken as JwtSecurityToken;

            if (token != null)
            {
                // Check if the token is active in the database
                var tokenString = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                bool isActive = await tokenService.IsTokenActive(tokenString);

                if (!isActive)
                {
                    context.Fail("Token has been revoked");
                }
            }
        }
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyGameApp API v1");
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
