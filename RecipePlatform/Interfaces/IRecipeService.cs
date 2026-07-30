using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Models;

namespace RecipePlatform.Api.Interfaces
{
	public interface IRecipeService
	{
		public Task<RecipeDto> CreateRecipeAsync(CreateRecipeRequest createRecipeRequest, CancellationToken cancellationToken);
		public Task<RecipeDto?> GetRecipeAsync(Guid id, CancellationToken cancellationToken);
		public Task<List<RecipeDto>> GetRecipesAsync(GetRecipesQuery query, CancellationToken cancellationToken);
		public Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken);
		public Task<RecipeDto?> UpdateRecipeAsync(UpdateRecipeRequest updateRecipeRequest, Guid id, CancellationToken cancellationToken);
	}
}
