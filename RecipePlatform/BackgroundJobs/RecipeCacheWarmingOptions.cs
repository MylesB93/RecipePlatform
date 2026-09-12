namespace RecipePlatform.Api.BackgroundJobs;

public sealed class RecipeCacheWarmingOptions
{
	public const string SectionName = "BackgroundJobs:RecipeCacheWarming";

	public int QueueCapacity { get; init; } = 100;
}
