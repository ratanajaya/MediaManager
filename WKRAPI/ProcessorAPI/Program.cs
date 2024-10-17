using Microsoft.OpenApi.Models;
using ProcessorAPI;
using Serilog;
using SharedLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProcessorAPI", Version = "v1" });
});

var logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "serilog/log-.txt"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

builder.Services.AddSingleton<Serilog.ILogger>(logger);
builder.Services.AddSingleton<ProcessorApiConfig>(new ProcessorApiConfig {
    TemDirPath = builder.Configuration.GetValue<string>("TempDirPath")!,
    TileSize = builder.Configuration.GetValue<int?>("TileSize")
});

builder.Services.AddScoped<ImageProcessor>();

builder.Services.AddCors(o => o.AddPolicy("AllowAllOrigin", builder => {
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           .WithExposedHeaders("Content-Disposition"); // to access fileName from server
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if(app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
