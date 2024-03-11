using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.PushMessages.Data.Repositories;

public class PushMessagesDbContext : DbContextWithTriggers
{
    public PushMessagesDbContext(DbContextOptions<PushMessagesDbContext> options)
        : base(options)
    {
    }

    protected PushMessagesDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<PushMessagesEntity>().ToTable("PushMessages").HasKey(x => x.Id);
        //modelBuilder.Entity<PushMessagesEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

        switch (Database.ProviderName)
        {
            case "Pomelo.EntityFrameworkCore.MySql":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.PushMessages.Data.MySql"));
                break;
            case "Npgsql.EntityFrameworkCore.PostgreSQL":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.PushMessages.Data.PostgreSql"));
                break;
            case "Microsoft.EntityFrameworkCore.SqlServer":
                modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.PushMessages.Data.SqlServer"));
                break;
        }
    }
}
