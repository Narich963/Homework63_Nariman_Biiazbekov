using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MyChat.Models;

public class MyChatContext : IdentityDbContext<User, IdentityRole<int>, int>
{
	public DbSet<User> Users { get; set; }
    public MyChatContext(DbContextOptions opts) : base(opts)
    {
        
    }
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
	}
}
