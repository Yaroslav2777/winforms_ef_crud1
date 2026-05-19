using Microsoft.EntityFrameworkCore;

namespace winforms_ef_crud
{
    internal class FitnessDbContext : DbContext
    {
        public DbSet<FitnessActivity> Activities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Connect to LocalDB instance using the provided connection string.
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=FitnessTrackerDB;Trusted_Connection=True;");
        }
    }
}
