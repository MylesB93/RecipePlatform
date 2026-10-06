namespace RecipePlatform.Api.ExternalRecipes;

public interface ITheMealDbClient
{
	Task<IReadOnlyList<TheMealDbMeal>> SearchMealsAsync(string name, CancellationToken cancellationToken);

	Task<TheMealDbMealDetails?> LookupMealAsync(string id, CancellationToken cancellationToken);
}
