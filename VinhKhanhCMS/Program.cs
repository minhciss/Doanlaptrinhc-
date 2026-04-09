using Microsoft.EntityFrameworkCore;
using VinhKhanhCMS.Data;
using VinhKhanhCMS.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5137); // 🔥 cho phép điện thoại truy cập
});

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=poi.db")); // ✅ lưu file thật

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TtsService>();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite("Data Source=poi.db"));

var app = builder.Build();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run("http://0.0.0.0:5137");