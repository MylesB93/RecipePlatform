using RecipePlatform.Api.ExternalRecipes;
using RecipePlatform.Api.Services;

namespace RecipePlatform.UnitTests.Tests;

public sealed class ExternalRecipeSearchServiceTests
{
	[Fact]
	public async Task SearchAsync_MapsMealDbResultsToApplicationResults()
	{
		var mealDbClient = new RecordingMealDbClient(
		[
			new TheMealDbMeal
			{
				Id = "52771",
				Name = "Spicy Arrabiata Penne",
				ThumbnailUrl = "https://example.test/meal.jpg"
			}
		]);
		var service = new ExternalRecipeSearchService(mealDbClient);

		var results = await service.SearchAsync("arrabiata", CancellationToken.None);

		var result = Assert.Single(results);
		Assert.Equal("52771", result.ExternalId);
		Assert.Equal("Spicy Arrabiata Penne", result.Name);
		Assert.Equal("https://example.test/meal.jpg", result.ThumbnailUrl);
		Assert.Equal("arrabiata", mealDbClient.SearchTerm);
	}

	private sealed class RecordingMealDbClient(IReadOnlyList<TheMealDbMeal> meals) : ITheMealDbClient
	{
		public string? SearchTerm { get; private set; }

		public Task<IReadOnlyList<TheMealDbMeal>> SearchMealsAsync(string name, CancellationToken cancellationToken)
		{
			SearchTerm = name;
			return Task.FromResult(meals);
		}
	}
}
