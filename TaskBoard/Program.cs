using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TaskBoard.Data;
using TaskBoard.EndpointService;
using TaskBoard.EndpointServices;
using TaskBoard.Logic;
using TaskBoard.LogicService;
using TaskBoard.WrapperService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsql => npgsql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null)
    ));
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthEndpointService, AuthEndpointService>();
builder.Services.AddScoped<IAuthLogic, AuthLogic>();
builder.Services.AddScoped<IAuthWrapperService, AuthWrapperService>();
builder.Services.AddScoped<IProjectsEndpointService, ProjectsEndpointService>();
builder.Services.AddScoped<IProjectsLogic, ProjectsLogic>();
builder.Services.AddScoped<IProjectsWrapperService, ProjectsWrapperService>();
builder.Services.AddScoped<IBoardEndpointService, BoardEndpointService>();
builder.Services.AddScoped<IBoardLogic, BoardLogic>();
builder.Services.AddScoped<IBoardWrapperService, BoardWrapperService>();
builder.Services.AddScoped<ITasksEndpointService, TasksEndpointService>();
builder.Services.AddScoped<ITasksLogic, TasksLogic>();
builder.Services.AddScoped<ITasksWrapperService, TasksWrapperService>();
builder.Services.AddScoped<ICommentsEndpointService, CommentsEndpointService>();
builder.Services.AddScoped<ICommentsLogic, CommentsLogic>();
builder.Services.AddScoped<ICommentsWrapperService, CommentsWrapperService>();
builder.Services.AddScoped<IActivityEndpointService, ActivityEndpointService>();
builder.Services.AddScoped<IActivityLogic, ActivityLogic>();
builder.Services.AddScoped<IActivityWrapperService, ActivityWrapperService>();
var jwtSecret = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("JWT secret is missing.");
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:5173"];

    if (allowedOrigins.Length == 0 || allowedOrigins.Any(string.IsNullOrWhiteSpace))
        throw new InvalidOperationException(
            "Configure at least one valid Cors:AllowedOrigins value.");

    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .WithHeaders("Authorization", "Content-Type", "Accept", "Origin")
            .AllowAnyMethod();
    });
});
builder.Services.AddAuthorization();
var app = builder.Build();

// Select the endpoint before applying CORS so endpoint metadata and CORS policy
// are evaluated together. CORS must still wrap authentication and authorization
// so their 401/403 responses include the appropriate CORS headers.
app.UseRouting();
app.UseCors("FrontendPolicy");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskBoard API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
