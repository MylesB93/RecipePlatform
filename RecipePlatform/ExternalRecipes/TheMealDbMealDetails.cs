using System.Text.Json;
using System.Text.Json.Serialization;

namespace RecipePlatform.Api.ExternalRecipes;

public sealed class TheMealDbMealDetails
{
	[JsonPropertyName("idMeal")]
	public required string Id { get; init; }

	[JsonPropertyName("strMeal")]
	public required string Name { get; init; }

	[JsonPropertyName("strCategory")]
	public string? Category { get; init; }

	[JsonPropertyName("strArea")]
	public string? Area { get; init; }

	[JsonPropertyName("strInstructions")]
	public string? Instructions { get; init; }

	[JsonPropertyName("strMealThumb")]
	public string? ThumbnailUrl { get; init; }

	[JsonPropertyName("strTags")]
	public string? Tags { get; init; }

	[JsonPropertyName("strYoutube")]
	public string? YoutubeUrl { get; init; }

	[JsonPropertyName("strSource")]
	public string? SourceUrl { get; init; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? AdditionalProperties { get; init; }
}
