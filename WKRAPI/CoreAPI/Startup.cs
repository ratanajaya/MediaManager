using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedLibrary;
using Serilog;
using CoreAPI.CustomMiddlewares;
using CoreAPI.AL.Models.Config;
using CoreAPI.AL.DataAccess;
using CoreAPI.AL.Services;
using CoreAPI.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using SharedLibrary.Models;
using CoreAPI.AL.Hubs;

namespace CoreAPI;

public class Startup
{
    public Startup(IConfiguration configuration) {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services) {
        services.AddControllers();
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });

        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        services.AddMemoryCache();

        services.AddCors(options => {
            options.AddPolicy("AllowAll",
                builder => {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .WithExposedHeaders("Content-Disposition");
                    // Note: Do not call AllowCredentials() when AllowAnyOrigin() is used
                });
        });

        #region Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "WKR", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter `Bearer` [space] and then your valid token in the text input below."
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
                    }, Array.Empty<string>()
                }
            });
        });
        #endregion

        #region Config & Logging
        var config = new CoreApiConfig {
            LibraryPath = Configuration.GetValue<string>("LibraryPath")!,
            ScLibraryPath = Configuration.GetValue<string>("ScLibraryPath")!,
            TempPath = Configuration.GetValue<string>("TempPath")!,
            PortableBrowserPath = Configuration.GetValue<string>("PortableBrowserPath")!,
            ProcessorApiUrl = Configuration.GetValue<string>("ProcessorApiUrl")!,

            Version = Configuration.GetValue<string>("Version")!,
            BuildType = Configuration.GetValue<string>("BuildType")!,
            AppType = "Api"
        };
        services.AddSingleton(config);

        services.AddSingleton(Configuration.GetSection("GoogleCred").Get<GoogleCred>()!);
        var authSetting = Configuration.GetSection("Auth").Get<AuthSetting>()!;
        services.AddSingleton(authSetting);

        var logger = new Func<ILogger>(() => {
            if(config.IsPublic) {
                return new LoggerConfiguration().CreateLogger();
            }
            else {
                return new LoggerConfiguration()
                    .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog/log-.txt"), rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            }
        })();

        services.AddSingleton<ILogger>(logger);
        #endregion

        #region Auth
        services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters() {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSetting.SecretKey))
            };
        });
        #endregion

        #region Services
        services.AddScoped<AuthorizationService>();
        services.AddScoped<FileRepository>();
        services.AddScoped<LibraryRepository>();
        services.AddScoped<ScRepository>();
        services.AddScoped<MediaProcessor>();
        services.AddScoped<ISystemIOAbstraction, SystemIOAbstraction>();
        services.AddScoped<ILogDbContext, LogDbContext>();
        services.AddScoped<IFlagDbContext, FlagDbContext>();
        services.AddScoped<IPcService, PcService>();
        services.AddScoped<ExtraInfoService>();
        services.AddScoped<StaticInfoService>();
        services.AddScoped<ImageProcessor>();
        services.AddScoped<OperationService>();
        services.AddScoped<AuthService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<GoogleService>();

        services.AddScoped<IDbContext, JsonDbContext>();
        services.AddScoped<CensorshipService>();
        services.AddSingleton(new Random());


        var ai = JsonSerializer.Deserialize<AlbumInfoProvider>(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "albumInfo.json")))!;
        services.AddSingleton<AlbumInfoProvider>(ai);
        #endregion
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
        app.UseRouting();
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<CustomExceptionMiddleware>();

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapHub<ProgressHub>("/progressHub")
                .RequireCors("AllowAll") // Apply CORS policy to the hub
                .AllowAnonymous();
        });

        if(env.IsDevelopment()) {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}