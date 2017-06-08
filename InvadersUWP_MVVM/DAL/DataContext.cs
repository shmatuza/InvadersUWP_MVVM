using System.Linq;
using InvadersUWP_MVVM.Model;
using Microsoft.EntityFrameworkCore;

namespace InvadersUWP_MVVM.DAL
{
    public class DataContext : DbContext
    {
        public DbSet<Highscore> Highscore { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=highscore.db");
        }
    }
}
