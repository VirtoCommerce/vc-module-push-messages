using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.MySql;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PushMessagesDbContext>
{
    public PushMessagesDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PushMessagesDbContext>();
        var connectionString = args.Any() ? args[0] : "Server=localhost;User=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseMySql(
            connectionString,
            ResolveServerVersion(args, connectionString),
            options => options.MigrationsAssembly(GetType().Assembly.GetName().Name));

        return new PushMessagesDbContext(builder.Options);
    }

    private static ServerVersion ResolveServerVersion(string[] args, string connectionString)
    {
        var serverVersion = args.Length >= 2 ? args[1] : null;

        if (serverVersion == "AutoDetect")
        {
            return ServerVersion.AutoDetect(connectionString);
        }

        if (serverVersion != null)
        {
            return ServerVersion.Parse(serverVersion);
        }

        return new MySqlServerVersion(new Version(5, 7));
    }
}
