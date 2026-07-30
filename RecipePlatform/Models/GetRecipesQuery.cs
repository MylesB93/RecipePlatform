namespace RecipePlatform.Api.Models;

public sealed record GetRecipesQuery(
	int Page = 1,
	int PageSize = 10,
	string? Search = null);