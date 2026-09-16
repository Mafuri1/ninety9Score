using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Score.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Controllers
// ------------------------------------------------------

builder.Services.AddControllers();


// ------------------------------------------------------
// Database
// ------------------------------------------------------

builder.Services.AddDbContext<ScoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ScoreDb")));


// ------------------------------------------------------
// JWT Authentication + Role Based Authorization
// ------------------------------------------------------

var requireAuthenticatedUser = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services
    .AddAuthorizationBuilder()
    //all endpoints require authentication by default, even if you accidentally forget [Authorize] on a new controller.
    .SetFallbackPolicy(requireAuthenticatedUser)
    .AddPolicy("WriteScores", policy =>
    {
        policy.RequireRole("admin");
    });

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization();


// ------------------------------------------------------
// Swagger
// ------------------------------------------------------

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Score API",
        Version = "v1",
        Description = "API for managing score information."
    });

    // Define JWT Bearer authentication
    options.AddSecurityDefinition(
        "bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT access token."
        });

    // Apply JWT authentication to Swagger operations
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "bearer",
                document)] = []
        });
});


// ------------------------------------------------------
// Build
// ------------------------------------------------------

var app = builder.Build();


// ------------------------------------------------------
// Swagger
// ------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Score API v1");

        options.RoutePrefix = "swagger";
    });
}


// ------------------------------------------------------
// Middleware
// ------------------------------------------------------

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();