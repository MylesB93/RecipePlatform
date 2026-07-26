using Microsoft.EntityFrameworkCore;
using RecipePlatform.Api.Data;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;

namespace RecipePlatform.Api.Services
{
	public class RecipeService : IRecipeService
	{
		private readonly RecipeDbContext _recipeDbContext;

		public RecipeService(RecipeDbContext recipeDbContext)
		{
			_recipeDbContext = recipeDbContext;
		}

		public async Task<RecipeDto> CreateRecipeAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<RecipeDto> GetRecipeAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<List<RecipeDto>> GetRecipesAsync(CancellationToken cancellationToken)
		{
			var recipes = await _recipeDbContext.Recipes
				.AsNoTracking()
				.ToListAsync(cancellationToken);

			var recipeDtos = recipes.Select(recipe => new RecipeDto
			{
				Id = recipe.Id,
				Name = recipe.Name,
				Description = recipe.Description
			}).ToList();

			return recipeDtos;
		}

		public async Task DeleteRecipeAsync()
		{
			throw new NotImplementedException();
		}

		public async Task<RecipeDto> UpdateRecipeAsync()
		{
			throw new NotImplementedException();
		}
	}
}
