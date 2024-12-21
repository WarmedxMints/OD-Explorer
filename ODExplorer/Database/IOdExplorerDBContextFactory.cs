namespace ODExplorer.Database
{
    public interface IOdExplorerDBContextFactory
    {
        ODExplorerDbContext CreateDbContext();
    }
}
