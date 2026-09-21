namespace RecipePlatform.Api.ExternalRecipes;

public sealed class TheMealDbOptions
{
	public const string SectionName = "TheMealDb";

	public required string BaseUrl { get; init; }

	public required string ApiKey { get; init; }
}
