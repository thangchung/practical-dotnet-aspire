using ProductApi.Domain;

namespace ProductApi.Infrastructure;

public class ProductDbContext(DbContextOptions<ProductDbContext> options) : DbContext(options)
{
	public required DbSet<ItemV2> Items { get; set; }

	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.HasPostgresExtension("vector");

		builder.Entity<ItemV2>().ToTable("products", "item");
		builder.Entity<ItemV2>().HasKey(x => x.Id);
		builder.Entity<ItemV2>().Ignore(x => x.DomainEvents);
		builder.Entity<ItemV2>().Property(x => x.Type).IsRequired();
		builder.Entity<ItemV2>().Property(x => x.Price).IsRequired();
		builder.Entity<ItemV2>().Property(x => x.Embedding).IsRequired();
	}
}
