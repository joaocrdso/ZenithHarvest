namespace ZenithHarvest.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ZenithContextFactory : IDesignTimeDbContextFactory<ZenithContext>
{
    public ZenithContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ZenithContext>();

        optionsBuilder.UseOracle(
            "Data Source=localhost:1521/freepdb1;User Id=zenith_user;Password=zenith123;");

        return new ZenithContext(optionsBuilder.Options);
    }
}