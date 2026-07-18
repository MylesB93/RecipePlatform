namespace RecipePlatform.Api.Data.DTOs
{
	public class RecipeDto
	{
		public Guid Id { get; set; }

		public required string Name { get; set; }

		public string? Description { get; set; }
	}
}
