using Microsoft.EntityFrameworkCore;
using RecipePlatform.Api.Data;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;
using RecipePlatform.Api.Migrations;
using RecipePlatform.Api.Models;

namespace RecipePlatform.Api.Services
{
	public class RecipeService : IRecipeService
	{
		private readonly RecipeDbContext _recipeDbContext;

		public RecipeService(RecipeDbContext recipeDbContext)
		{
			_recipeDbContext = recipeDbContext;
		}

		public async Task<RecipeDto> CreateRecipeAsync(CreateRecipeRequest createRecipeRequest, CancellationToken cancellationToken)
		{
			var recipe = new Recipe
			{
				Id = Guid.NewGuid(),
				Name = createRecipeRequest.Name.Trim(),
				Description = createRecipeRequest.Description?.Trim()
			};

			_recipeDbContext.Recipes.Add(recipe);
			await _recipeDbContext.SaveChangesAsync(cancellationToken);

			var recipeDto = new RecipeDto
			{
				Id = recipe.Id,
				Name = recipe.Name,
				Description = recipe.Description
			};

			return recipeDto;
		}

		public async Task<RecipeDto?> GetRecipeAsync(Guid id, CancellationToken cancellationToken)
		{
			var recipe = await _recipeDbContext.Recipes
			.AsNoTracking()
			.SingleOrDefaultAsync(
				x => x.Id == id,
				cancellationToken);

			var recipeDto = recipe is null
				? null
				: new RecipeDto
				{
					Id = recipe.Id,
					Name = recipe.Name,
					Description = recipe.Description
				};

			return recipeDto;
		}

		public async Task<List<RecipeDto>> GetRecipesAsync(GetRecipesQuery query, CancellationToken cancellationToken)
		{
			var recipes = _recipeDbContext.Recipes
				.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(query.Search))
			{
				recipes = recipes.Where(recipe =>
					EF.Functions.ILike(recipe.Name, $"%{query.Search}%"));
			}

			var result = await recipes
				.OrderBy(r => r.Name)
				.Skip((query.Page - 1) * query.PageSize)
				.Take(query.PageSize)
				.Select(recipe => new RecipeDto
				{
					Id = recipe.Id,
					Name = recipe.Name,
					Description = recipe.Description
				})
				.ToListAsync(cancellationToken);

			return result;
		}

		public async Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken)
		{
			var recipe = await _recipeDbContext.Recipes
			.SingleOrDefaultAsync(
				x => x.Id == id,
				cancellationToken);
			if (recipe is null)
			{
				return false;
			}
			_recipeDbContext.Recipes.Remove(recipe);
			await _recipeDbContext.SaveChangesAsync(cancellationToken);
			return true;
		}

		public async Task<RecipeDto?> UpdateRecipeAsync(UpdateRecipeRequest updateRecipeRequest, Guid id, CancellationToken cancellationToken)
		{
			var recipe = await _recipeDbContext.Recipes.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
			if (recipe == null)
			{
				return null;
			}

			if (!string.IsNullOrWhiteSpace(updateRecipeRequest.Name) && recipe.Name != updateRecipeRequest.Name)
			{
				recipe.Name = updateRecipeRequest.Name;
			}

			if (!string.IsNullOrWhiteSpace(updateRecipeRequest.Description) && recipe.Description != updateRecipeRequest.Description)
			{
				recipe.Description = updateRecipeRequest.Description;
			}

			await _recipeDbContext.SaveChangesAsync(cancellationToken);

			var recipeDto = new RecipeDto() { Name = recipe.Name, Description = recipe.Description };

			return recipeDto;
		}
	}
}
