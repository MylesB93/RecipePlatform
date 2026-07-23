namespace RecipePlatform.Api.Models
{
	public sealed record RecipeResponse(
		Guid Id,
		string Name,
		string Description);
}
