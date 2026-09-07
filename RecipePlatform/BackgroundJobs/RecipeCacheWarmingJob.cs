namespace RecipePlatform.Api.BackgroundJobs;

public sealed record RecipeCacheWarmingJob(Guid RecipeId, Guid Version);
