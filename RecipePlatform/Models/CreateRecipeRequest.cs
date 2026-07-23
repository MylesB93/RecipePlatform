namespace RecipePlatform.Api.Models
{
	public sealed record CreateRecipeRequest(
		string Name,
		string? Description);
}