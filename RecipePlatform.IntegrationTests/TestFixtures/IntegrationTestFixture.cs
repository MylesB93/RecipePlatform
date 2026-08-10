using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RecipePlatform.Api.Data;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace RecipePlatform.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
	private readonly PostgreSqlContainer _postgresContainer =
		new PostgreSqlBuilder()
			.WithImage("postgres:17")
			.WithDatabase("recipes_test")
			.WithUsername("postgres")
			.WithPassword("postgres")
			.Build();
	private readonly RedisContainer _redisContainer =
		new RedisBuilder("redis:7-alpine").Build();

	private WebApplicationFactory<Program>? _factory;

	public HttpClient Client { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		await _postgresContainer.StartAsync();
		await _redisContainer.StartAsync();

		var connectionString = _postgresContainer.GetConnectionString();

		_factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.UseEnvironment("Testing");

				builder.ConfigureAppConfiguration((_, configuration) =>
				{
					configuration.AddInMemoryCollection(
						new Dictionary<string, string?>
						{
							["ConnectionStrings:Postgres"] = connectionString,
							["ConnectionStrings:Redis"] = _redisContainer.GetConnectionString()
						});
				});

				builder.ConfigureTestServices(services =>
				{
					services.RemoveAll<IDistributedCache>();
					services.AddStackExchangeRedisCache(options =>
					{
						options.Configuration = _redisContainer.GetConnectionString();
						options.InstanceName = "recipe-platform:";
					});
				});
			});

		Client = _factory.CreateClient();

		using var scope = _factory.Services.CreateScope();


		var configuration =
			scope.ServiceProvider.GetRequiredService<IConfiguration>();

		var configuredConnectionString =
			configuration.GetConnectionString("Postgres");

		Console.WriteLine("CONFIGURED POSTGRES: " + configuredConnectionString);
		Console.WriteLine("POSTGRES: " + _postgresContainer.GetConnectionString());
		Console.WriteLine("CONFIGURED REDIS: " + configuration.GetConnectionString("Redis"));
		Console.WriteLine("REDIS: " + _redisContainer.GetConnectionString());


		var dbContext =
			scope.ServiceProvider.GetRequiredService<RecipeDbContext>();

		await dbContext.Database.MigrateAsync();
	}

	public async Task DisposeAsync()
	{
		Client.Dispose();

		if (_factory is not null)
		{
			await _factory.DisposeAsync();
		}

		await _postgresContainer.DisposeAsync();
		await _redisContainer.DisposeAsync();
	}

	public async Task ResetDatabaseAsync()
	{
		using IServiceScope scope = _factory.Services.CreateScope();

		RecipeDbContext dbContext =
			scope.ServiceProvider.GetRequiredService<RecipeDbContext>();

		dbContext.Recipes.RemoveRange(dbContext.Recipes);

		await dbContext.SaveChangesAsync();
	}
}
