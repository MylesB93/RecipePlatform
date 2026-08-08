using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
			});

		Client = _factory.CreateClient();

		using var scope = _factory.Services.CreateScope();


		var configuration =
			scope.ServiceProvider.GetRequiredService<IConfiguration>();

		var configuredConnectionString =
			configuration.GetConnectionString("Postgres");

		Console.WriteLine("CONFIGURED: " + configuredConnectionString);
		Console.WriteLine("POSTGRES: " + _postgresContainer.GetConnectionString());


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
