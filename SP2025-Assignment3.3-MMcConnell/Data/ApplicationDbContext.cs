using Microsoft.EntityFrameworkCore;
using SP2025_Assignment3._3_MMcConnell.Models;

namespace SP2025_Assignment3._3_MMcConnell.Data
{
    public class ApplicationDbContext : DbContext 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<MovieActor> MovieActor {  get; set; }
    }
}
