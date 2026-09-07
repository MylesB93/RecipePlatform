using RecipePlatform.Api.BackgroundJobs;

namespace RecipePlatform.UnitTests.Tests;

public sealed class RecipeCacheWarmingQueueTests
{
	[Fact]
	public async Task DequeueAsync_WhenJobHasBeenEnqueued_ReturnsThatJob()
	{
		var queue = new RecipeCacheWarmingQueue();
		var job = new RecipeCacheWarmingJob(Guid.NewGuid(), Guid.NewGuid());

		bool queued = queue.TryEnqueue(job);
		RecipeCacheWarmingJob dequeuedJob = await queue.DequeueAsync(CancellationToken.None);

		Assert.True(queued);
		Assert.Equal(job, dequeuedJob);
	}
}
