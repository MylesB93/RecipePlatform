namespace RecipePlatform.Api.Models;

public sealed class Recipe
{
	public Guid Id { get; set; }

	public required string Name { get; set; }

	public string? Description { get; set; }
}