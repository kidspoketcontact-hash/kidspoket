using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Tests;

// כל טסט מקבל KidsPocketDbContext טרי מגובה InMemory - EnsureCreated מחיל גם את
// שלוש קופות ברירת המחדל שנזרעות דרך HasData ב-OnModelCreating.
public static class TestDb
{
    public static KidsPocketDbContext Create()
    {
        var options = new DbContextOptionsBuilder<KidsPocketDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new KidsPocketDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
