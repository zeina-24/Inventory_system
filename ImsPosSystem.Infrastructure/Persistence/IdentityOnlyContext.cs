using ImsPosSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ImsPosSystem.Infrastructure.Persistence;

public class IdentityOnlyContext : IdentityDbContext<ApplicationUser>
{
    public IdentityOnlyContext(DbContextOptions<IdentityOnlyContext> options)
        : base(options)
    {
    }
}

public class IdentityOnlyContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<IdentityOnlyContext>
{
    public IdentityOnlyContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityOnlyContext>();
        optionsBuilder.UseSqlServer("Server=DESKTOP-GF59DLG;Database=Inventory_management_system;Trusted_Connection=True;TrustServerCertificate=True;");
        return new IdentityOnlyContext(optionsBuilder.Options);
    }
}
