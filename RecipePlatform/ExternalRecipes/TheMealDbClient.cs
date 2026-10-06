using System.Net.Http.Json;

namespace RecipePlatform.Api.ExternalRecipes;

public sealed class TheMealDbClient(HttpClient httpClient) : ITheMealDbClient
{
	public async Task<IReadOnlyList<TheMealDbMeal>> SearchMealsAsync(string name, CancellationToken cancellationToken)
	{
		using HttpResponseMessage response = await httpClient.GetAsync(
			$"search.php?s={Uri.EscapeDataString(name)}",
			cancellationToken);
		response.EnsureSuccessStatusCode();

		TheMealDbSearchResponse? searchResponse = await response.Content.ReadFromJsonAsync<TheMealDbSearchResponse>(cancellationToken);
		return searchResponse?.Meals ?? [];
	}

	public async Task<TheMealDbMealDetails?> LookupMealAsync(string id, CancellationToken cancellationToken)
	{
		using HttpResponseMessage response = await httpClient.GetAsync(
			$"lookup.php?i={Uri.EscapeDataString(id)}",
			cancellationToken);
		response.EnsureSuccessStatusCode();

		TheMealDbLookupResponse? lookupResponse = await response.Content.ReadFromJsonAsync<TheMealDbLookupResponse>(cancellationToken);
		return lookupResponse?.Meals?.FirstOrDefault();
	}
}
