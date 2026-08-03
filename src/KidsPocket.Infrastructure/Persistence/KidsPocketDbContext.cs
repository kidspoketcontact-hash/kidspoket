using KidsPocket.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Infrastructure.Persistence;

public class KidsPocketDbContext : DbContext
{
    public KidsPocketDbContext(DbContextOptions<KidsPocketDbContext> options) : base(options) { }

    public DbSet<Household> Households => Set<Household>();
    public DbSet<Adult> Adults => Set<Adult>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<ChildAdult> ChildAdults => Set<ChildAdult>();
    public DbSet<Bucket> Buckets => Set<Bucket>();
    public DbSet<Ledger> Ledgers => Set<Ledger>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<MoneyEvent> MoneyEvents => Set<MoneyEvent>();
    public DbSet<Decision> Decisions => Set<Decision>();
    public DbSet<DecisionAllocation> DecisionAllocations => Set<DecisionAllocation>();
    public DbSet<DecisionReflection> DecisionReflections => Set<DecisionReflection>();
    public DbSet<AllowancePlan> AllowancePlans => Set<AllowancePlan>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();
    public DbSet<Chore> Chores => Set<Chore>();
    public DbSet<Reward> Rewards => Set<Reward>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Household>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.HasMany(x => x.Adults).WithOne(a => a.Household).HasForeignKey(a => a.HouseholdId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.ChildMemberships).WithOne(cm => cm.Household).HasForeignKey(cm => cm.HouseholdId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Adult>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email);
        });

        modelBuilder.Entity<Child>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
            e.HasOne(x => x.Ledger).WithOne(l => l.Child!).HasForeignKey<Ledger>(l => l.ChildId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.AdultMemberships).WithOne(cm => cm.Child).HasForeignKey(cm => cm.ChildId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.SavingGoals).WithOne(g => g.Child).HasForeignKey(g => g.ChildId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Chores).WithOne(c => c.Child).HasForeignKey(c => c.ChildId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChildAdult>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ChildId, x.HouseholdId }).IsUnique();
        });

        modelBuilder.Entity<Bucket>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).IsRequired().HasMaxLength(50);
            e.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
            e.HasIndex(x => x.Code).IsUnique();

            // שלוש קופות ברירת המחדל נזרעות ישירות במיגרציה - Enjoy / Save / Grow
            e.HasData(
                new { Id = BucketIds.Enjoy, Code = "enjoy", DisplayName = "Enjoy", IsSystemDefault = true, SortOrder = 1 },
                new { Id = BucketIds.Save, Code = "save", DisplayName = "Save", IsSystemDefault = true, SortOrder = 2 },
                new { Id = BucketIds.Grow, Code = "grow", DisplayName = "Grow", IsSystemDefault = true, SortOrder = 3 }
            );
        });

        modelBuilder.Entity<Ledger>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Entries).WithOne(en => en.Ledger).HasForeignKey(en => en.LedgerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasOne(x => x.Bucket).WithMany().HasForeignKey(x => x.BucketId).OnDelete(DeleteBehavior.Restrict);
            // Append-only: אין Update, רק Insert - נאכף ברמת האפליקציה (אין מתודות שינוי בישות עצמה)
        });

        modelBuilder.Entity<MoneyEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Decision>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.MoneyEvent).WithMany().HasForeignKey(x => x.MoneyEventId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Allocations).WithOne(a => a.Decision).HasForeignKey(a => a.DecisionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Reflection).WithOne(r => r.Decision!).HasForeignKey<DecisionReflection>(r => r.DecisionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DecisionAllocation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.HasOne(x => x.Bucket).WithMany().HasForeignKey(x => x.BucketId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DecisionReflection>(e =>
        {
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<AllowancePlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<SavingGoal>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.TargetAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Chore>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.RewardAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Reward>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
        });
    }
}
