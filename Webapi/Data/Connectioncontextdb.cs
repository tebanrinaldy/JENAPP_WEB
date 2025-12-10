using Microsoft.EntityFrameworkCore;
using Webapi.Models;

namespace Webapi.Data
{
    public class Connectioncontextdb: DbContext
    {
        public Connectioncontextdb(DbContextOptions<Connectioncontextdb> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; } 
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<Webapi.Models.InventoryMovement> InventoryMovement { get; set; } = default!;
        public DbSet<PendingSale> PendingSales { get; set; }
        public DbSet<PendingSaleDetail> PendingSaleDetails { get; set; }
    }

}
