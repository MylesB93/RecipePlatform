namespace RecipePlatform.Api.ExternalRecipes;

public interface ITheMealDbClient
{
	Task<IReadOnlyList<TheMealDbMeal>> SearchMealsAsync(string name, CancellationToken cancellationToken);
}
