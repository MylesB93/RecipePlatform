using System.Text.Json.Serialization;

namespace RecipePlatform.Api.ExternalRecipes;

public sealed class TheMealDbSearchResponse
{
	[JsonPropertyName("meals")]
	public IReadOnlyList<TheMealDbMeal>? Meals { get; init; }
}
