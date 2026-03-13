using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Project.Api.JWT;
using Project.Api.Middlewares;
using System.Reflection;
using System.Text;

namespace Project.Api;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddHttpContextAccessor();

        // --- JWT ---
        var jwtConfig = _configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt config section is missing.");
        services.AddSingleton(jwtConfig);
        services.AddScoped<IJwtService, JwtTokenGenerator>();

        var key = Encoding.UTF8.GetBytes(jwtConfig.SecretKey);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtConfig.Issuer,
                ValidAudience            = jwtConfig.Audience,
                IssuerSigningKey         = new SymmetricSecurityKey(key)
            };
        });

        // --- Infrastructure (Repositories, Services, DB) ---
        // Defined in Project.Api/Extensions/ServiceExtensions.cs
        services.AddInfrastructure(_configuration);

        // --- Controllers ---
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        // --- Swagger ---
        // Requires <GenerateDocumentationFile>true</GenerateDocumentationFile> in .csproj
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title   = "Project API",
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.ApiKey,
                Scheme      = "Bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "JWT Authorization header. Example: 'Bearer {token}'"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        // --- Exception Handling ---
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // --- CORS ---
        var corsSettings = _configuration.GetSection("Cors");
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .WithOrigins(corsSettings.GetSection("AllowedOrigins").Get<string[]>()
                        ?? throw new InvalidOperationException("Cors:AllowedOrigins is missing."))
                    .WithMethods(corsSettings.GetSection("AllowedMethods").Get<string[]>()
                        ?? throw new InvalidOperationException("Cors:AllowedMethods is missing."))
                    .WithHeaders(corsSettings.GetSection("AllowedHeaders").Get<string[]>()
                        ?? throw new InvalidOperationException("Cors:AllowedHeaders is missing."));
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
        app.UseCors("CorsPolicy");
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
