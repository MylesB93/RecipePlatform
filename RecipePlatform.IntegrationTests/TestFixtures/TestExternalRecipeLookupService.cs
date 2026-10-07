using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;

namespace RecipePlatform.IntegrationTests;

public sealed class TestExternalRecipeLookupService : IExternalRecipeLookupService
{
	public Task<ExternalRecipeDetails?> LookupAsync(string externalId, CancellationToken cancellationToken)
	{
		if (externalId == "provider-failure")
		{
			throw new HttpRequestException("TheMealDB is unavailable.");
		}

		if (externalId == "not-found")
		{
			return Task.FromResult<ExternalRecipeDetails?>(null);
		}

		return Task.FromResult<ExternalRecipeDetails?>(
			new ExternalRecipeDetails(
				externalId,
				"Spicy Arrabiata Penne",
				"Vegetarian",
				"Italian",
				"Cook pasta.",
				"https://example.test/meal.jpg",
				["Pasta", "Italian"],
				null,
				null,
				[new ExternalRecipeIngredient("penne rigate", "1 pound")]));
	}
}
