using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.PushMessages.Data.Models;

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

        modelBuilder.Entity<PushMessageEntity>().ToTable("PushMessage").HasKey(x => x.Id);
        modelBuilder.Entity<PushMessageEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

        modelBuilder.Entity<PushMessageMemberEntity>().ToTable("PushMessageMember").HasKey(x => x.Id);
        modelBuilder.Entity<PushMessageMemberEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
        modelBuilder.Entity<PushMessageMemberEntity>().HasOne(x => x.Message).WithMany(x => x.Members)
            .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<PushMessageMemberEntity>()
            .HasIndex(x => new { x.MessageId, x.MemberId })
            .IsUnique()
            .HasDatabaseName("IX_PushMessageMember_MessageId_MemberId");

        modelBuilder.Entity<PushMessageRecipientEntity>().ToTable("PushMessageRecipient").HasKey(x => x.Id);
        modelBuilder.Entity<PushMessageRecipientEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
        modelBuilder.Entity<PushMessageRecipientEntity>().HasOne(x => x.Message).WithMany(x => x.Recipients)
            .HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade).IsRequired();
        modelBuilder.Entity<PushMessageRecipientEntity>()
            .HasIndex(x => new { x.MessageId, x.UserId })
            .IsUnique()
            .HasDatabaseName("IX_PushMessageRecipient_MessageId_UserId");

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
