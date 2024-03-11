using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.PushMessages.Data.PostgreSql;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Configures the context to use PostgreSQL.
    /// </summary>
    public static DbContextOptionsBuilder UsePostgreSqlDatabase(this DbContextOptionsBuilder builder, string connectionString) =>
        builder.UseNpgsql(
            connectionString,
            options => options.MigrationsAssembly(typeof(DesignTimeDbContextFactory).Assembly.GetName().Name));
}
