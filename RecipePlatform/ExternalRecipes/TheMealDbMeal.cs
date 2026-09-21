using System.Text.Json.Serialization;

namespace RecipePlatform.Api.ExternalRecipes;

public sealed class TheMealDbMeal
{
	[JsonPropertyName("idMeal")]
	public required string Id { get; init; }

	[JsonPropertyName("strMeal")]
	public required string Name { get; init; }

	[JsonPropertyName("strMealThumb")]
	public string? ThumbnailUrl { get; init; }
}
