using InventorySystemBackend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InventorySystemBackend.Data
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration config;

        public DatabaseContext(IConfiguration configuration)
        {
            config = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(config.GetConnectionString("ConnString"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Inventory>()
                .HasKey(i => new { i.product_id, i.branch_id });
        }

        public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }
        public DbSet<EmployeeAuth> EmployeeAuths { get; set; }        
        public DbSet<Branches> Branches { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<SoldItems> SoldItems { get; set; }
        public DbSet<Requests> Requests { get; set; }
        public DbSet<AuditLogs> AuditLogs { get; set; }
        public DbSet<ArchiveAccounts> ArchiveAccounts { get; set; }
        public DbSet<ArchiveProducts> ArchiveProducts { get; set; }
    }
}
