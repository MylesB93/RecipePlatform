using System.Text.Json.Serialization;

namespace RecipePlatform.Api.ExternalRecipes;

public sealed class TheMealDbLookupResponse
{
	[JsonPropertyName("meals")]
	public IReadOnlyList<TheMealDbMealDetails>? Meals { get; init; }
}
