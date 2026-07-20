using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using RecipePlatform.Api.Data;
using Testcontainers.PostgreSql;

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

	private WebApplicationFactory<Program>? _factory;

	public HttpClient Client { get; private set; } = null!;

	public async Task InitializeAsync()
	{
		// Start the temporary PostgreSQL Docker container.
		await _postgresContainer.StartAsync();

		// Create a test version of the API.
		_factory = new WebApplicationFactory<Program>()
			.WithWebHostBuilder(builder =>
			{
				builder.UseEnvironment("Testing");

				builder.ConfigureServices(services =>
				{
					// Remove the DbContext registration from Program.cs.
					services.RemoveAll<DbContextOptions<RecipeDbContext>>();

					// Replace it with a connection to the test container.
					services.AddDbContext<RecipeDbContext>(options =>
					{
						options.UseNpgsql(
							_postgresContainer.GetConnectionString());
					});
				});
			});

		Client = _factory.CreateClient();

		// Apply the application's EF Core migrations to the test database.
		using IServiceScope scope = _factory.Services.CreateScope();

		RecipeDbContext dbContext =
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
	}
}