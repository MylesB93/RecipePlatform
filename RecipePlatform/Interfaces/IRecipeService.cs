using RecipePlatform.Api.Data.DTOs;

namespace RecipePlatform.Api.Interfaces
{
	public interface IRecipeService
	{
		public Task<RecipeDto> CreateRecipeAsync();
		public Task<RecipeDto> GetRecipeAsync();
		public Task<List<RecipeDto>> GetRecipesAsync(CancellationToken cancellationToken);
		public Task DeleteRecipeAsync();
		public Task<RecipeDto> UpdateRecipeAsync();
	}
}
