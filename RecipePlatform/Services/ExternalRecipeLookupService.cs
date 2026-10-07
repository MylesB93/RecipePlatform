using System.Text.Json;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.ExternalRecipes;
using RecipePlatform.Api.Interfaces;

namespace RecipePlatform.Api.Services;

public sealed class ExternalRecipeLookupService(ITheMealDbClient theMealDbClient) : IExternalRecipeLookupService
{
	public async Task<ExternalRecipeDetails?> LookupAsync(string externalId, CancellationToken cancellationToken)
	{
		TheMealDbMealDetails? meal = await theMealDbClient.LookupMealAsync(externalId, cancellationToken);
		if (meal is null) return null;

		return new ExternalRecipeDetails(
			meal.Id,
			meal.Name,
			meal.Category,
			meal.Area,
			meal.Instructions,
			meal.ThumbnailUrl,
			ParseTags(meal.Tags),
			meal.YoutubeUrl,
			meal.SourceUrl,
			ParseIngredients(meal.AdditionalProperties));
	}

	private static IReadOnlyList<string> ParseTags(string? tags) =>
		string.IsNullOrWhiteSpace(tags)
			? []
			: tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

	private static IReadOnlyList<ExternalRecipeIngredient> ParseIngredients(Dictionary<string, JsonElement>? properties)
	{
		var ingredients = new List<ExternalRecipeIngredient>();

		for (var index = 1; index <= 20; index++)
		{
			string? name = GetPropertyValue(properties, $"strIngredient{index}");
			if (name is null) continue;

			ingredients.Add(new ExternalRecipeIngredient(
				name,
				GetPropertyValue(properties, $"strMeasure{index}")));
		}

		return ingredients;
	}

	private static string? GetPropertyValue(Dictionary<string, JsonElement>? properties, string propertyName)
	{
		if (properties is null ||
			!properties.TryGetValue(propertyName, out JsonElement value) ||
			value.ValueKind is not JsonValueKind.String)
		{
			return null;
		}

		string? text = value.GetString()?.Trim();
		return string.IsNullOrEmpty(text) ? null : text;
	}
}
