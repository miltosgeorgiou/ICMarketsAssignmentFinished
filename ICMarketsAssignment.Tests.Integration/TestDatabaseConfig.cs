using ICMarketsAssignment.AppDatabaseContext;
using ICMarketsAssignment.Repositories;
using ICMarketsAssignment.Repositories.impl;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ICMarketsAssignment.Tests.Integration
{
    public static class TestDatabaseConfig
    {
        public static (SqliteConnection Conn, DatabaseContext Db, ISymbolRepository Repo) Create()
        {
            var conn = new SqliteConnection("DataSource=:memory:");
            conn.Open();

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite(conn)
                .Options;

            var db = new DatabaseContext(options);

            //To create the schemas
            db.Database.EnsureCreated();

            ISymbolRepository repo = new SymbolRepository(db);

            return (conn, db, repo);
        }
    }
}
