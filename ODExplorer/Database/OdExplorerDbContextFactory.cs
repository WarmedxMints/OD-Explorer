using Microsoft.EntityFrameworkCore;

namespace ODExplorer.Database
{
    public sealed class OdExplorerDbContextFactory(string connectionString) : IOdExplorerDBContextFactory
    {
        private readonly string _connectionString = connectionString;
        public ODExplorerDbContext CreateDbContext()
        {
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(_connectionString).Options;

            var context = new ODExplorerDbContext(options);
            using var connection = context.Database.GetDbConnection();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "pragma journal_mode = WAL;PRAGMA synchronous = normal;pragma temp_store = memory;pragma mmap_size = 30000000000;";
                command.ExecuteNonQuery();
            }
            connection.Close();
            return context;
        }
    }
}
