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
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// DI
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IStorageService, CloudinaryStorageService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddHostedService<CleanupHostedService>();

// CORS — chỉ đăng ký DUY NHẤT policy "FrontendPolicy"
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://magnificent-motivation-production-ccf8.up.railway.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
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

// CORS phải đứng TRƯỚC MapControllers, và đúng tên policy đã đăng ký ở trên
app.UseCors("FrontendPolicy");

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
