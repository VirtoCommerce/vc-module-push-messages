using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.PushMessages.Data.MySql;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the context to use MySQL.
    /// </summary>
    public static DbContextOptionsBuilder UseMySqlDatabase(this DbContextOptionsBuilder builder, string connectionString) =>
        builder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            options => options.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
}
