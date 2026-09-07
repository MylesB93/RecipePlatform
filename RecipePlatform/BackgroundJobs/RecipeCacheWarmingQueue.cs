using System.Threading.Channels;

namespace RecipePlatform.Api.BackgroundJobs;

public sealed class RecipeCacheWarmingQueue : IRecipeCacheWarmingQueue
{
	private readonly Channel<RecipeCacheWarmingJob> _jobs = Channel.CreateBounded<RecipeCacheWarmingJob>(
		new BoundedChannelOptions(100)
		{
			FullMode = BoundedChannelFullMode.Wait,
			SingleReader = true
		});

	public bool TryEnqueue(RecipeCacheWarmingJob job) => _jobs.Writer.TryWrite(job);

	public ValueTask<RecipeCacheWarmingJob> DequeueAsync(CancellationToken cancellationToken) =>
		_jobs.Reader.ReadAsync(cancellationToken);
}
