using RecipePlatform.Api.Models;
using System.Net;
using System.Net.Http.Json;

namespace RecipePlatform.IntegrationTests.Tests;

public sealed class RecipeEndpointsTests
: IClassFixture<IntegrationTestFixture>
{
	private readonly HttpClient _client;

	public RecipeEndpointsTests(
		IntegrationTestFixture fixture)
	{
		_client = fixture.Client;
	}

	[Fact]
	public async Task CreateRecipe_WithValidRequest_PersistsAndReturnsRecipe()
	{
		// Arrange
		var request = new CreateRecipeRequest(
			"Chicken Curry",
			"A simple chicken curry recipe.");

		// Act
		HttpResponseMessage createResponse =
			await _client.PostAsJsonAsync(
				"/api/recipes",
				request);

		// Assert create response
		Assert.Equal(
			HttpStatusCode.Created,
			createResponse.StatusCode);

		RecipeResponse? createdRecipe =
			await createResponse.Content
				.ReadFromJsonAsync<RecipeResponse>();

		Assert.NotNull(createdRecipe);
		Assert.NotEqual(Guid.Empty, createdRecipe.Id);
		Assert.Equal(request.Name, createdRecipe.Name);
		Assert.Equal(
			request.Description,
			createdRecipe.Description);

		// Act: retrieve the persisted recipe
		HttpResponseMessage getResponse =
			await _client.GetAsync(
				$"/api/recipes/{createdRecipe.Id}");

		// Assert retrieve response
		Assert.Equal(
			HttpStatusCode.OK,
			getResponse.StatusCode);

		RecipeResponse? retrievedRecipe =
			await getResponse.Content
				.ReadFromJsonAsync<RecipeResponse>();

		Assert.NotNull(retrievedRecipe);
		Assert.Equal(createdRecipe.Id, retrievedRecipe.Id);
		Assert.Equal(request.Name, retrievedRecipe.Name);
		Assert.Equal(
			request.Description,
			retrievedRecipe.Description);
	}

	[Fact]
	public async Task GetRecipe_WithInvalidRequest_Returns404()
	{
		// Arrange
		var InvalidId = "unknownId";

		// Act
		HttpResponseMessage responseMessage =
			await _client.GetAsync(
				$"/api/recipes/{InvalidId}");

		// Assert
		Assert.Equal(HttpStatusCode.NotFound, responseMessage.StatusCode);
	}

	[Fact]
	public async Task PostRecipe_WithNoName_Returns400()
	{
		// Arrange
		var request = new CreateRecipeRequest("", "Invalid recipe");

		// Act
		var response = await _client.PostAsJsonAsync($"/api/recipes", request);

		// Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task DeleteRecipe_WithValidRequest_RemovesRecipeAndReturns204()
	{
		// Arrange
		var createRequest = new CreateRecipeRequest("Pancakes", "Some delicious pancakes");

		// Act create response
		var createResponse = await _client.PostAsJsonAsync($"/api/recipes", createRequest);

		// Assert response is valid
		Assert.NotNull(createResponse);
		Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

		// Arrange 
		var deletedRecipe = await createResponse.Content.ReadFromJsonAsync<RecipeResponse>();

		// Act delete recipe
		var deletedResponse = await _client.DeleteAsync($"/api/recipes/{deletedRecipe?.Id}");

		// Assert
		Assert.Equal(HttpStatusCode.NoContent, deletedResponse.StatusCode);
	}
}
