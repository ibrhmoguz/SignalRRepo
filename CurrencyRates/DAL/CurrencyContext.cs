namespace DAL
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class CurrencyContext : DbContext
    {
        public CurrencyContext()
            : base("name=CurrencyContext")
        {
        }

        public virtual DbSet<CurrencyReport> CurrencyReport { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CurrencyReport>()
                .Property(e => e.CurrencyName)
                .IsUnicode(false);

            modelBuilder.Entity<CurrencyReport>()
                .Property(e => e.ForexBuying)
                .HasPrecision(18, 5);

            modelBuilder.Entity<CurrencyReport>()
                .Property(e => e.ForexSelling)
                .HasPrecision(18, 5);

            modelBuilder.Entity<CurrencyReport>()
                .Property(e => e.BanknoteBuying)
                .HasPrecision(18, 5);

            modelBuilder.Entity<CurrencyReport>()
                .Property(e => e.BanknoteSelling)
                .HasPrecision(18, 5);

            modelBuilder.Entity<CurrencyReport>()
                .Property(e => e.CrossRateUSD)
                .HasPrecision(18, 5);
        }
    }
}
