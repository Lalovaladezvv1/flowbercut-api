using Microsoft.EntityFrameworkCore;

namespace Flowbercut.Api.Data;

public sealed class FlowbercutDbContext : DbContext
{
    public FlowbercutDbContext(
        DbContextOptions<FlowbercutDbContext> options)
        : base(options)
    {
    }
}
