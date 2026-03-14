using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Library.Domain;

namespace Library.MVC.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 1 Invoice -> Many Lines
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(l => l.Invoice)
                .WithMany(i => i.InvoiceLines)
                .HasForeignKey(l => l.InvoiceId);

            // 1 Product -> Many Lines
            modelBuilder.Entity<InvoiceLine>()
                .HasOne(l => l.Product)
                .WithMany(p => p.InvoiceLines)
                .HasForeignKey(l => l.ProductId);

        }
    }
}
