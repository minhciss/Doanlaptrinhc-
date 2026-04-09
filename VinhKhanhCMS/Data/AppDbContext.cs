using Microsoft.EntityFrameworkCore;
using VinhKhanhCMS.Models;

namespace VinhKhanhCMS.Data;

public class AppDbContext : DbContext
{
    // ✅ PHẢI CÓ constructor này
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Poi> Pois { get; set; }
    public DbSet<PoiTranslation> PoiTranslations { get; set; }
}