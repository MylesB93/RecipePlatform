using System.Net;

namespace RecipePlatform.IntegrationTests;

public sealed class HealthEndpointTests
	: IClassFixture<IntegrationTestFixture>
{
	private readonly HttpClient _client;

	public HealthEndpointTests(IntegrationTestFixture fixture)
	{
		_client = fixture.Client;
	}

	[Fact]
	public async Task GetHealth_WhenApplicationIsRunning_ReturnsOk() //TODO: figure out why this is failing
	{
		// Act
		HttpResponseMessage response =
			await _client.GetAsync("/health");

		// Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		string content = await response.Content.ReadAsStringAsync();

		Assert.Equal("Healthy", content);
	}
}