using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace RecipePlatform.Api.BackgroundJobs;

public sealed class RecipeCacheWarmingQueue : IRecipeCacheWarmingQueue
{
	private readonly Channel<RecipeCacheWarmingJob> _jobs;

	public RecipeCacheWarmingQueue(IOptions<RecipeCacheWarmingOptions> options)
	{
		_jobs = Channel.CreateBounded<RecipeCacheWarmingJob>(
			new BoundedChannelOptions(options.Value.QueueCapacity)
		{
			FullMode = BoundedChannelFullMode.Wait,
			SingleReader = true
			});
	}

	public bool TryEnqueue(RecipeCacheWarmingJob job) => _jobs.Writer.TryWrite(job);

	public ValueTask<RecipeCacheWarmingJob> DequeueAsync(CancellationToken cancellationToken) =>
		_jobs.Reader.ReadAsync(cancellationToken);
}
