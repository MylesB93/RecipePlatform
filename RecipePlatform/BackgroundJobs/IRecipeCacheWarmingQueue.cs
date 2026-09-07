namespace RecipePlatform.Api.BackgroundJobs;

public interface IRecipeCacheWarmingQueue
{
	bool TryEnqueue(RecipeCacheWarmingJob job);

	ValueTask<RecipeCacheWarmingJob> DequeueAsync(CancellationToken cancellationToken);
}
