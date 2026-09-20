using Microsoft.EntityFrameworkCore;
using FITBOOM.Models;

namespace FITBOOM.Data
{
    public class FITBOOMContext : DbContext
    {
        public FITBOOMContext(DbContextOptions<FITBOOMContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
    }
}