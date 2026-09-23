using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.ExternalRecipes;
using RecipePlatform.Api.Interfaces;
using Serilog;

namespace RecipePlatform.Api.Services;

public sealed class CachedExternalRecipeSearchService(
	ExternalRecipeSearchService externalRecipeSearchService,
	IDistributedCache cache,
	IOptions<TheMealDbOptions> options) : IExternalRecipeSearchService
{
	private readonly DistributedCacheEntryOptions _cacheOptions = new()
	{
		AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.Value.SearchCacheDurationMinutes)
	};

	public async Task<IReadOnlyList<ExternalRecipeSearchResult>> SearchAsync(string name, CancellationToken cancellationToken)
	{
		string cacheKey = CacheKey(name);
		IReadOnlyList<ExternalRecipeSearchResult>? cachedResults = await GetAsync(cacheKey, cancellationToken);
		if (cachedResults is not null) return cachedResults;

		IReadOnlyList<ExternalRecipeSearchResult> results = await externalRecipeSearchService.SearchAsync(name, cancellationToken);
		await SetAsync(cacheKey, results, cancellationToken);
		return results;
	}

	private async Task<IReadOnlyList<ExternalRecipeSearchResult>?> GetAsync(string cacheKey, CancellationToken cancellationToken)
	{
		try
		{
			string? value = await cache.GetStringAsync(cacheKey, cancellationToken);
			return value is null
				? null
				: JsonSerializer.Deserialize<List<ExternalRecipeSearchResult>>(value);
		}
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "External recipe search cache read failed. {CacheKey}", cacheKey);
			return null;
		}
	}

	private async Task SetAsync(string cacheKey, IReadOnlyList<ExternalRecipeSearchResult> results, CancellationToken cancellationToken)
	{
		try
		{
			await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(results), _cacheOptions, cancellationToken);
		}
		catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
		{
			Log.Warning(exception, "External recipe search cache write failed. {CacheKey}", cacheKey);
		}
	}

	private static string CacheKey(string name) =>
		$"external-recipes:search:{Uri.EscapeDataString(name.Trim().ToLowerInvariant())}";
}
