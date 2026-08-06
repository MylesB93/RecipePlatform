using Microsoft.EntityFrameworkCore;
using RecipePlatform.Api.Data;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Models;
using RecipePlatform.Api.Services;

namespace RecipePlatform.UnitTests.Tests;

public sealed class RecipeServiceTests
{
	[Fact]
	public async Task CreateRecipeAsync_TrimsValuesAndPersistsRecipe()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		var service = new RecipeService(dbContext);

		RecipeDto recipe = await service.CreateRecipeAsync(
			new CreateRecipeRequest("  Pancakes  ", "  Fluffy pancakes  "),
			CancellationToken.None);

		Assert.NotEqual(Guid.Empty, recipe.Id);
		Assert.Equal("Pancakes", recipe.Name);
		Assert.Equal("Fluffy pancakes", recipe.Description);

		Recipe persistedRecipe = await dbContext.Recipes.SingleAsync();
		Assert.Equal(recipe.Id, persistedRecipe.Id);
		Assert.Equal("Pancakes", persistedRecipe.Name);
		Assert.Equal("Fluffy pancakes", persistedRecipe.Description);
	}

	[Fact]
	public async Task GetRecipeAsync_WhenRecipeExists_ReturnsMappedDto()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		Recipe recipe = AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var service = new RecipeService(dbContext);

		RecipeDto? result = await service.GetRecipeAsync(recipe.Id, CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(recipe.Id, result.Id);
		Assert.Equal(recipe.Name, result.Name);
		Assert.Equal(recipe.Description, result.Description);
	}

	[Fact]
	public async Task GetRecipeAsync_WhenRecipeDoesNotExist_ReturnsNull()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		var service = new RecipeService(dbContext);

		RecipeDto? result = await service.GetRecipeAsync(Guid.NewGuid(), CancellationToken.None);

		Assert.Null(result);
	}

	[Fact]
	public async Task DeleteRecipeAsync_WhenRecipeExists_RemovesRecipe()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		Recipe recipe = AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var service = new RecipeService(dbContext);

		bool result = await service.DeleteRecipeAsync(recipe.Id, CancellationToken.None);

		Assert.True(result);
		Assert.Empty(await dbContext.Recipes.ToListAsync());
	}

	[Fact]
	public async Task DeleteRecipeAsync_WhenRecipeDoesNotExist_ReturnsFalse()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		var service = new RecipeService(dbContext);

		bool result = await service.DeleteRecipeAsync(Guid.NewGuid(), CancellationToken.None);

		Assert.False(result);
	}

	[Fact]
	public async Task UpdateRecipeAsync_WhenRecipeExists_UpdatesValuesAndReturnsMappedDto()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		Recipe recipe = AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var service = new RecipeService(dbContext);

		RecipeDto? result = await service.UpdateRecipeAsync(
			new UpdateRecipeRequest("Waffles", "Crispy waffles"),
			recipe.Id,
			CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal(recipe.Id, result.Id);
		Assert.Equal("Waffles", result.Name);
		Assert.Equal("Crispy waffles", result.Description);

		Recipe persistedRecipe = await dbContext.Recipes.SingleAsync();
		Assert.Equal("Waffles", persistedRecipe.Name);
		Assert.Equal("Crispy waffles", persistedRecipe.Description);
	}

	[Fact]
	public async Task UpdateRecipeAsync_WithBlankValues_PreservesExistingValues()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		Recipe recipe = AddRecipe(dbContext, "Pancakes", "Fluffy pancakes");
		var service = new RecipeService(dbContext);

		RecipeDto? result = await service.UpdateRecipeAsync(
			new UpdateRecipeRequest(" ", " "),
			recipe.Id,
			CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal("Pancakes", result.Name);
		Assert.Equal("Fluffy pancakes", result.Description);
	}

	[Fact]
	public async Task UpdateRecipeAsync_WhenRecipeDoesNotExist_ReturnsNull()
	{
		await using RecipeDbContext dbContext = CreateDbContext();
		var service = new RecipeService(dbContext);

		RecipeDto? result = await service.UpdateRecipeAsync(
			new UpdateRecipeRequest("Pancakes", "Fluffy pancakes"),
			Guid.NewGuid(),
			CancellationToken.None);

		Assert.Null(result);
	}

	private static RecipeDbContext CreateDbContext()
	{
		DbContextOptions<RecipeDbContext> options =
			new DbContextOptionsBuilder<RecipeDbContext>()
				.UseInMemoryDatabase(Guid.NewGuid().ToString())
				.Options;

		return new RecipeDbContext(options);
	}

	private static Recipe AddRecipe(RecipeDbContext dbContext, string name, string? description)
	{
		var recipe = new Recipe
		{
			Id = Guid.NewGuid(),
			Name = name,
			Description = description
		};

		dbContext.Recipes.Add(recipe);
		dbContext.SaveChanges();

		return recipe;
	}
}
