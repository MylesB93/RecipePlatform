using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RecipePlatform.Api.BackgroundJobs;
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
		var cachedService = CreateCachedService(dbContext);

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
		var cachedService = CreateCachedService(dbContext);
		var query = new GetRecipesQuery();

		List<RecipeDto> initialRecipes = await cachedService.GetRecipesAsync(query, CancellationToken.None);
		await cachedService.CreateRecipeAsync(
			new CreateRecipeRequest("Waffles", "Crispy waffles"),
			CancellationToken.None);
		List<RecipeDto> refreshedRecipes = await cachedService.GetRecipesAsync(query, CancellationToken.None);

		Assert.Single(initialRecipes);
		Assert.Equal(2, refreshedRecipes.Count);
	}

	[Fact]
	public async Task UpdateRecipeAsync_WhenUpdateSucceeds_QueuesCacheWarmingJob()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		Recipe recipe = AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var queue = new RecordingRecipeCacheWarmingQueue();
		var cachedService = CreateCachedService(dbContext, queue);

		RecipeDto? updatedRecipe = await cachedService.UpdateRecipeAsync(
			new UpdateRecipeRequest("Blueberry pancakes", "Fluffy pancakes with blueberries", recipe.Version),
			recipe.Id,
			CancellationToken.None);

		Assert.NotNull(updatedRecipe);
		RecipeCacheWarmingJob job = Assert.Single(queue.Jobs);
		Assert.Equal(updatedRecipe.Id, job.RecipeId);
		Assert.Equal(updatedRecipe.Version, job.Version);
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

	private static CachedRecipeService CreateCachedService(
		RecipeDbContext dbContext,
		IRecipeCacheWarmingQueue? queue = null) =>
		new(new RecipeService(dbContext), CreateCache(), queue ?? new RecordingRecipeCacheWarmingQueue());

	private static Recipe AddRecipe(RecipeDbContext dbContext, string name, string? description)
	{
		var recipe = new Recipe { Id = Guid.NewGuid(), Name = name, Description = description };
		dbContext.Recipes.Add(recipe);
		dbContext.SaveChanges();
		return recipe;
	}

	private sealed class RecordingRecipeCacheWarmingQueue : IRecipeCacheWarmingQueue
	{
		public List<RecipeCacheWarmingJob> Jobs { get; } = [];

		public bool TryEnqueue(RecipeCacheWarmingJob job)
		{
			Jobs.Add(job);
			return true;
		}

		public ValueTask<RecipeCacheWarmingJob> DequeueAsync(CancellationToken cancellationToken) =>
			throw new NotSupportedException();
	}
}
