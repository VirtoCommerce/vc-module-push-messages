using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.PostgreSql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PushMessagesDbContext>
{
    public PushMessagesDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PushMessagesDbContext>();
        var connectionString = args.Any() ? args[0] : "Server=localhost;Username=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(GetType().Assembly.GetName().Name));

        return new PushMessagesDbContext(builder.Options);
    }
}
