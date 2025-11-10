using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace User.Infrastructure.Persistence
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            
            var cs = Environment.GetEnvironmentVariable("POSTGRES_CS");

            if (string.IsNullOrWhiteSpace(cs))
                cs = "Host=localhost;Port=5432;Database=UserServiceDb;Username=postgres;Password=Gurgen2003%";

            var opts = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(cs)
                .Options;

            return new AppDbContext(opts);
        }
    }
}
