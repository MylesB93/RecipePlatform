namespace RecipePlatform.Api.Data.DTOs;

public sealed record ExternalRecipeDetails(
	string ExternalId,
	string Name,
	string? Category,
	string? Area,
	string? Instructions,
	string? ThumbnailUrl,
	IReadOnlyList<string> Tags,
	string? YoutubeUrl,
	string? SourceUrl,
	IReadOnlyList<ExternalRecipeIngredient> Ingredients);
