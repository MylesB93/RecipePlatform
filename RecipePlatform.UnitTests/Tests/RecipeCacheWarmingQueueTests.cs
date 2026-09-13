using RecipePlatform.Api.BackgroundJobs;
using Microsoft.Extensions.Options;

namespace RecipePlatform.UnitTests.Tests;

public sealed class RecipeCacheWarmingQueueTests
{
	[Fact]
	public async Task DequeueAsync_WhenJobHasBeenEnqueued_ReturnsThatJob()
	{
		var queue = CreateQueue();
		var job = new RecipeCacheWarmingJob(Guid.NewGuid(), Guid.NewGuid());

		bool queued = queue.TryEnqueue(job);
		RecipeCacheWarmingJob dequeuedJob = await queue.DequeueAsync(CancellationToken.None);

		Assert.True(queued);
		Assert.Equal(job, dequeuedJob);
	}

	[Fact]
	public void TryEnqueue_WhenQueueHasReachedConfiguredCapacity_ReturnsFalse()
	{
		var queue = CreateQueue(queueCapacity: 1);

		bool firstJobQueued = queue.TryEnqueue(new RecipeCacheWarmingJob(Guid.NewGuid(), Guid.NewGuid()));
		bool secondJobQueued = queue.TryEnqueue(new RecipeCacheWarmingJob(Guid.NewGuid(), Guid.NewGuid()));

		Assert.True(firstJobQueued);
		Assert.False(secondJobQueued);
	}

	private static RecipeCacheWarmingQueue CreateQueue(int queueCapacity = 100) =>
		new(Options.Create(new RecipeCacheWarmingOptions { QueueCapacity = queueCapacity }));
}
