using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Apolon.Data;

public class ApolonDbContextFactory : IDesignTimeDbContextFactory<ApolonDbContext>
{
    public ApolonDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "Host=aws-0-eu-west-1.pooler.supabase.com;Port=5432;Database=postgres;" +
            "Username=postgres.znhnbvekblwydwqcqkmf;Password=pppk-projekt123;" +
            "SSL Mode=Require;Trust Server Certificate=true";

        var optionsBuilder = new DbContextOptionsBuilder<ApolonDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ApolonDbContext(optionsBuilder.Options);
    }
}