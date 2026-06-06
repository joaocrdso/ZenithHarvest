namespace ZenithHarvest.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ZenithContextFactory : IDesignTimeDbContextFactory<ZenithContext>
{
    public ZenithContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ZenithContext>();
        optionsBuilder.UseOracle("User Id=ZENITH;Password=PLACEHOLDER;Data Source=localhost:1521/XEPDB1;");
        
        return new ZenithContext(optionsBuilder.Options);
    }
}
