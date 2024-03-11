using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.SqlServer;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PushMessagesDbContext>
{
    public PushMessagesDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PushMessagesDbContext>();
        var connectionString = args.Length != 0 ? args[0] : "Server=(local);User=virto;Password=virto;Database=VirtoCommerce3;";

        builder.UseSqlServer(
            connectionString,
            options => options.MigrationsAssembly(GetType().Assembly.GetName().Name));

        return new PushMessagesDbContext(builder.Options);
    }
}
