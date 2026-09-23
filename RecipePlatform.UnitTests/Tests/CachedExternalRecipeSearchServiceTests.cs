using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.ExternalRecipes;
using RecipePlatform.Api.Services;

namespace RecipePlatform.UnitTests.Tests;

public sealed class CachedExternalRecipeSearchServiceTests
{
	[Fact]
	public async Task SearchAsync_WhenSameNormalisedSearchWasCached_ReturnsCachedResults()
	{
		var mealDbClient = new CountingMealDbClient();
		var service = new CachedExternalRecipeSearchService(
			new ExternalRecipeSearchService(mealDbClient),
			CreateCache());

		IReadOnlyList<ExternalRecipeSearchResult> firstResults = await service.SearchAsync("Arrabiata", CancellationToken.None);
		IReadOnlyList<ExternalRecipeSearchResult> secondResults = await service.SearchAsync(" arrabiata ", CancellationToken.None);

		Assert.Single(firstResults);
		Assert.Single(secondResults);
		Assert.Equal(1, mealDbClient.SearchCount);
	}

	private static IDistributedCache CreateCache() =>
		new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));

	private sealed class CountingMealDbClient : ITheMealDbClient
	{
		public int SearchCount { get; private set; }

		public Task<IReadOnlyList<TheMealDbMeal>> SearchMealsAsync(string name, CancellationToken cancellationToken)
		{
			SearchCount++;
			return Task.FromResult<IReadOnlyList<TheMealDbMeal>>(
			[
				new TheMealDbMeal { Id = "52771", Name = "Spicy Arrabiata Penne" }
			]);
		}
	}
}
