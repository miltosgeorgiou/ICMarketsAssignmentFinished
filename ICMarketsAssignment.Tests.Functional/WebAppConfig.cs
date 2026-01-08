using ICMarketsAssignment.AppDatabaseContext;
using ICMarketsAssignment.HttpClients;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ICMarketsAssignment.Tests.Functional
{
    public class WebAppConfig : WebApplicationFactory<Program>
    {
        private SqliteConnection? _conn;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBlockCypherClient));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IBlockCypherClient, MockBlockCypherClient>();
                services.RemoveAll(typeof(DbContextOptions<DatabaseContext>));
                _conn = new SqliteConnection("DataSource=:memory:");
                _conn.Open();

                services.AddDbContext<DatabaseContext>(options =>
                {
                    options.UseSqlite(_conn);
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                db.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _conn?.Dispose();
                _conn = null;
            }
        }
    }
}
