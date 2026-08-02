using FileSharing.API.Data;
using FileSharing.API.Interfaces;
using FileSharing.API.Repositories;
using FileSharing.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// DI
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHostedService<CleanupHostedService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
            "http://localhost:5173",
            "https://magnificent-motivation-production-ccf8.up.railway.app")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowFrontend");
app.MapControllers();
app.Run();
