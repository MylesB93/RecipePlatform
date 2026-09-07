using Microsoft.EntityFrameworkCore;
using RecipePlatform.Api.Models;

namespace RecipePlatform.Api.Data;

public sealed class RecipeDbContext(
	DbContextOptions<RecipeDbContext> options)
	: DbContext(options)
{
	public DbSet<Recipe> Recipes => Set<Recipe>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Recipe>()
			.Property(recipe => recipe.Version)
			.IsConcurrencyToken();

		base.OnModelCreating(modelBuilder);
	}
}
