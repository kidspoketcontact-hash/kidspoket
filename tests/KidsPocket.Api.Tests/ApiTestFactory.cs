using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KidsPocket.Api.Tests;

// מחליף את חיבור ה-Npgsql האמיתי ב-EF InMemory, כדי שהטסטים ירוצו בלי Postgres מקומי.
// כל אינסטנס של Factory מקבל שם DB ייחודי כדי שטסטים לא ישתפו מצב ביניהם.
public class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // AddDbContext לא מחליף קונפיגורציה קודמת - הוא צובר אותה דרך
            // IDbContextOptionsConfiguration<T>, אז ה-Npgsql וה-InMemory שניהם היו מוחלים יחד
            // אם לא מסירים גם את זה, לא רק את ה-DbContextOptions<T> עצמו.
            services.RemoveAll<DbContextOptions<KidsPocketDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<KidsPocketDbContext>>();
            services.AddDbContext<KidsPocketDbContext>(options => options.UseInMemoryDatabase(_dbName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<KidsPocketDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
