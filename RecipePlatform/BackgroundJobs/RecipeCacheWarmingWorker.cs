using RecipePlatform.Api.Interfaces;
using RecipePlatform.Api.Models;

namespace RecipePlatform.Api.BackgroundJobs;

public sealed class RecipeCacheWarmingWorker(
	IRecipeCacheWarmingQueue queue,
	IServiceScopeFactory serviceScopeFactory,
	ILogger<RecipeCacheWarmingWorker> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				RecipeCacheWarmingJob job = await queue.DequeueAsync(stoppingToken);
				await WarmDefaultRecipeListAsync(job, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "Recipe cache warming job failed.");
			}
		}
	}

	private async Task WarmDefaultRecipeListAsync(RecipeCacheWarmingJob job, CancellationToken cancellationToken)
	{
		using IServiceScope scope = serviceScopeFactory.CreateScope();
		IRecipeService recipeService = scope.ServiceProvider.GetRequiredService<IRecipeService>();

		await recipeService.GetRecipesAsync(new GetRecipesQuery(), cancellationToken);

		logger.LogInformation(
			"Recipe cache warming job completed. {RecipeId} {RecipeVersion}",
			job.RecipeId,
			job.Version);
	}
}
