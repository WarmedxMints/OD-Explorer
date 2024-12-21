using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ODExplorer.Database
{
    public sealed class ODDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ODExplorerDbContext>
    {
        public ODExplorerDbContext CreateDbContext(string[] args)
        {
            var dbOptions = new DbContextOptionsBuilder().UseSqlite("DataSource=ODExplorer.db;").Options;
            return new ODExplorerDbContext(dbOptions);
        }
    }
}
