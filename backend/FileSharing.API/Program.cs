using FileSharing.API.Data;
using FileSharing.API.Interfaces;
using FileSharing.API.Repositories;
using FileSharing.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DbContext
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"]
                    ?? builder.Configuration["ConnectionStrings__DefaultConnection"]
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>

// DI
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHostedService<CleanupHostedService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760; // 10 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10485760; // 10 MB
});

var app = builder.Build();

// // Turn on CORS right at the top of the pipeline
app.UseCors("AllowAll");

// Auto migrate on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Warning] Error Migrate DB: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
