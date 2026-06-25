/*using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Career_Guidance_Platform.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            "server=127.0.0.1;port=3306;database=ProductDbb;user=root;password=Mothaiba123@@";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36))
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}*/