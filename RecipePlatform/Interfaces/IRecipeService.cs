using RecipePlatform.Api.Data.DTOs;

namespace RecipePlatform.Api.Interfaces
{
	public interface IRecipeService
	{
		public Task<RecipeDto> CreateRecipeAsync();
		public Task<RecipeDto?> GetRecipeAsync(Guid id, CancellationToken cancellationToken);
		public Task<List<RecipeDto>> GetRecipesAsync(CancellationToken cancellationToken);
		public Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken);
		public Task<RecipeDto> UpdateRecipeAsync();
	}
}
