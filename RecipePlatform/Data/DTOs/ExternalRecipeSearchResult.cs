namespace RecipePlatform.Api.Data.DTOs;

public sealed record ExternalRecipeSearchResult(
	string ExternalId,
	string Name,
	string? ThumbnailUrl);
