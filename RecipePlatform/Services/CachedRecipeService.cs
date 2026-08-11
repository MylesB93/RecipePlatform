using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;
using RecipePlatform.Api.Models;
using Serilog;

namespace RecipePlatform.Api.Services;

public sealed class CachedRecipeService(
	RecipeService recipeService,
	IDistributedCache cache) : IRecipeService
{
	private const string ListGenerationKey = "recipes:list:generation";
	private static readonly DistributedCacheEntryOptions RecipeCacheOptions = new()
	{
		AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
	};
	private static readonly DistributedCacheEntryOptions ListCacheOptions = new()
	{
		AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
	};

	public async Task<RecipeDto> CreateRecipeAsync(CreateRecipeRequest request, CancellationToken cancellationToken)
	{
		RecipeDto recipe = await recipeService.CreateRecipeAsync(request, cancellationToken);
		await SetAsync(RecipeKey(recipe.Id), recipe, RecipeCacheOptions, cancellationToken);
		await InvalidateListsAsync(cancellationToken);
		return recipe;
	}

	public async Task<RecipeDto?> GetRecipeAsync(Guid id, CancellationToken cancellationToken)
	{
		RecipeDto? cachedRecipe = await GetAsync<RecipeDto>(RecipeKey(id), cancellationToken);
		if (cachedRecipe is not null) return cachedRecipe;
		Log.Debug("Recipe cache miss. {RecipeId}", id);

		RecipeDto? recipe = await recipeService.GetRecipeAsync(id, cancellationToken);
		if (recipe is not null) await SetAsync(RecipeKey(id), recipe, RecipeCacheOptions, cancellationToken);
		return recipe;
	}

	public async Task<List<RecipeDto>> GetRecipesAsync(GetRecipesQuery query, CancellationToken cancellationToken)
	{
		string generation = await GetListGenerationAsync(cancellationToken);
		string key = $"recipes:list:{generation}:{query.Page}:{query.PageSize}:{Uri.EscapeDataString(query.Search?.Trim().ToLowerInvariant() ?? string.Empty)}";
		List<RecipeDto>? cachedRecipes = await GetAsync<List<RecipeDto>>(key, cancellationToken);
		if (cachedRecipes is not null) return cachedRecipes;
		Log.Debug("Recipe list cache miss. {CacheKey}", key);

		List<RecipeDto> recipes = await recipeService.GetRecipesAsync(query, cancellationToken);
		await SetAsync(key, recipes, ListCacheOptions, cancellationToken);
		return recipes;
	}

	public async Task<bool> DeleteRecipeAsync(Guid id, CancellationToken cancellationToken)
	{
		bool deleted = await recipeService.DeleteRecipeAsync(id, cancellationToken);
		if (!deleted) return false;

		try { await cache.RemoveAsync(RecipeKey(id), cancellationToken); }
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "Cache removal failed. {CacheKey}", RecipeKey(id));
		}
		await InvalidateListsAsync(cancellationToken);
		return true;
	}

	public async Task<RecipeDto?> UpdateRecipeAsync(UpdateRecipeRequest request, Guid id, CancellationToken cancellationToken)
	{
		RecipeDto? recipe = await recipeService.UpdateRecipeAsync(request, id, cancellationToken);
		if (recipe is null) return null;

		await SetAsync(RecipeKey(id), recipe, RecipeCacheOptions, cancellationToken);
		await InvalidateListsAsync(cancellationToken);
		return recipe;
	}

	private async Task<string> GetListGenerationAsync(CancellationToken cancellationToken)
	{
		try
		{
			string? generation = await cache.GetStringAsync(ListGenerationKey, cancellationToken);
			if (!string.IsNullOrEmpty(generation)) return generation;

			generation = Guid.NewGuid().ToString("N");
			await cache.SetStringAsync(ListGenerationKey, generation, cancellationToken);
			return generation;
		}
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "Cache generation lookup failed; bypassing list cache.");
			return Guid.NewGuid().ToString("N");
		}
	}

	private async Task InvalidateListsAsync(CancellationToken cancellationToken)
	{
		try { await cache.SetStringAsync(ListGenerationKey, Guid.NewGuid().ToString("N"), cancellationToken); }
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "Recipe list cache invalidation failed.");
		}
	}

	private async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
	{
		try
		{
			string? value = await cache.GetStringAsync(key, cancellationToken);
			return value is null ? default : JsonSerializer.Deserialize<T>(value);
		}
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "Cache read failed; falling back to the database. {CacheKey}", key);
			return default;
		}
	}

	private async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken)
	{
		try { await cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, cancellationToken); }
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "Cache write failed. {CacheKey}", key);
		}
	}

	private static string RecipeKey(Guid id) => $"recipes:item:{id}";
}
