using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RecipePlatform.Api.Data;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Models;
using RecipePlatform.Api.Services;

namespace RecipePlatform.UnitTests.Tests;

public sealed class CachedRecipeServiceTests
{
	[Fact]
	public async Task GetRecipeAsync_WhenRecipeIsCached_ReturnsCachedRecipeAfterDatabaseDeletion()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		Recipe recipe = AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var cachedService = new CachedRecipeService(new RecipeService(dbContext), CreateCache());

		RecipeDto? firstResult = await cachedService.GetRecipeAsync(recipe.Id, CancellationToken.None);
		dbContext.Recipes.Remove(recipe);
		await dbContext.SaveChangesAsync();
		RecipeDto? secondResult = await cachedService.GetRecipeAsync(recipe.Id, CancellationToken.None);

		Assert.NotNull(firstResult);
		Assert.NotNull(secondResult);
		Assert.Equal(recipe.Id, secondResult.Id);
	}

	[Fact]
	public async Task CreateRecipeAsync_InvalidatesCachedRecipeLists()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var cachedService = new CachedRecipeService(new RecipeService(dbContext), CreateCache());
		var query = new GetRecipesQuery();

		List<RecipeDto> initialRecipes = await cachedService.GetRecipesAsync(query, CancellationToken.None);
		await cachedService.CreateRecipeAsync(
			new CreateRecipeRequest("Waffles", "Crispy waffles"),
			CancellationToken.None);
		List<RecipeDto> refreshedRecipes = await cachedService.GetRecipesAsync(query, CancellationToken.None);

		Assert.Single(initialRecipes);
		Assert.Equal(2, refreshedRecipes.Count);
	}

	private static RecipeDbContext CreateDbContext()
	{
		DbContextOptions<RecipeDbContext> options = new DbContextOptionsBuilder<RecipeDbContext>()
			.UseInMemoryDatabase(Guid.NewGuid().ToString())
			.Options;
		return new RecipeDbContext(options);
	}

	private static IDistributedCache CreateCache() =>
		new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

	private static Recipe AddRecipe(RecipeDbContext dbContext, string name, string? description)
	{
		var recipe = new Recipe { Id = Guid.NewGuid(), Name = name, Description = description };
		dbContext.Recipes.Add(recipe);
		dbContext.SaveChanges();
		return recipe;
	}
}
