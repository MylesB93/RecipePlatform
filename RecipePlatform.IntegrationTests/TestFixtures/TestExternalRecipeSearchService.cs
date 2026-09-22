using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;

namespace RecipePlatform.IntegrationTests;

public sealed class TestExternalRecipeSearchService : IExternalRecipeSearchService
{
	public Task<IReadOnlyList<ExternalRecipeSearchResult>> SearchAsync(string name, CancellationToken cancellationToken)
	{
		if (name == "provider-failure")
		{
			throw new HttpRequestException("TheMealDB is unavailable.");
		}

		return Task.FromResult<IReadOnlyList<ExternalRecipeSearchResult>>(
		[
			new ExternalRecipeSearchResult("test-meal", $"{name} meal", "https://example.test/meal.jpg")
		]);
	}
}
