using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.ExternalRecipes;
using RecipePlatform.Api.Interfaces;

namespace RecipePlatform.Api.Services;

public sealed class ExternalRecipeSearchService(ITheMealDbClient theMealDbClient) : IExternalRecipeSearchService
{
	public async Task<IReadOnlyList<ExternalRecipeSearchResult>> SearchAsync(string name, CancellationToken cancellationToken)
	{
		IReadOnlyList<TheMealDbMeal> meals = await theMealDbClient.SearchMealsAsync(name, cancellationToken);

		return meals
			.Select(meal => new ExternalRecipeSearchResult(meal.Id, meal.Name, meal.ThumbnailUrl))
			.ToList();
	}
}
