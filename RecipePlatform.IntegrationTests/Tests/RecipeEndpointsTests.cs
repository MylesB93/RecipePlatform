using RecipePlatform.Api.Models;
using System.Net;
using System.Net.Http.Json;

namespace RecipePlatform.IntegrationTests.Tests;

public sealed class RecipeEndpointsTests
: IClassFixture<IntegrationTestFixture>, IAsyncLifetime
{
	private readonly HttpClient _client;
	private readonly IntegrationTestFixture _fixture;

	public RecipeEndpointsTests(
		IntegrationTestFixture fixture)
	{
		_fixture = fixture;
		_client = _fixture.Client;
	}

	public async Task InitializeAsync()
	{
		await _fixture.ResetDatabaseAsync();
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
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

	[Fact]
	public async Task UpdateRecipe_WithUpdatedDescription_PersistsAndReturnsRecipe()
	{
		// Arrange
		var createRequest = new CreateRecipeRequest("New Recipe", "This is a new recipe");

		// Act
		var createResponse = await _client.PostAsJsonAsync("/api/recipes", createRequest);

		// Assert
		Assert.NotNull(createResponse);
		Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

		// Arrange
		var recipe = await createResponse.Content.ReadFromJsonAsync<RecipeResponse>();
		var updateDescription = "This is an updated recipe.";
		var updateRequest = new UpdateRecipeRequest("", updateDescription);

		// Act
		var updateResponse = await _client.PutAsJsonAsync($"/api/recipes/{recipe?.Id}", updateRequest);
		var updatedRecipe = await updateResponse.Content.ReadFromJsonAsync<RecipeResponse>();

		// Assert
		Assert.NotNull(updatedRecipe);
		Assert.Equal(updatedRecipe.Name, recipe?.Name);
		Assert.Equal(updatedRecipe.Description, updateDescription);
	}

	[Fact]
	public async Task GetRecipes_WithValidRequest_ReturnsExpectedRecipes() // TODO: figure out why this is failing
	{
		// Arrange
		var steakRecipeRequest = new CreateRecipeRequest("Steak & Potatoes", "Sirloin steak with roast potatoes.");
		var katsuRecipeRequest = new CreateRecipeRequest("Chicken Katsu", "Japanese-style chicken katsu with rice.");
		var hotDogRecipeRequest = new CreateRecipeRequest("Hot Dogs", "Hot dog sausages in hot dog buns.");

		// Act
		var steakRecipeResponse = await _client.PostAsJsonAsync("/api/recipes/", steakRecipeRequest);
		var katsuRecipeResponse = await _client.PostAsJsonAsync("/api/recipes/", katsuRecipeRequest);
		var hotDogRecipeResponse = await _client.PostAsJsonAsync("/api/recipes/", hotDogRecipeRequest);

		// Assert
		Assert.NotNull(steakRecipeResponse);
		Assert.NotNull(katsuRecipeResponse);
		Assert.NotNull(hotDogRecipeResponse);
		Assert.Equal(HttpStatusCode.Created, steakRecipeResponse.StatusCode);
		Assert.Equal(HttpStatusCode.Created, katsuRecipeResponse.StatusCode);
		Assert.Equal(HttpStatusCode.Created, hotDogRecipeResponse.StatusCode);

		// Act
		var recipesResponse = await _client.GetFromJsonAsync<List<RecipeResponse>>("/api/recipes/");

		// Assert 
		Assert.NotNull(recipesResponse);
		Assert.NotEmpty(recipesResponse);
		Assert.Equal(3, recipesResponse.Count);
		Assert.Equal(katsuRecipeRequest.Name, recipesResponse[0].Name);
		Assert.Equal(hotDogRecipeRequest.Name, recipesResponse[1].Name);
		Assert.Equal(steakRecipeRequest.Name, recipesResponse[2].Name);
	}

	// TODO: implement this
	[Fact]
	public async Task GetRecipes_WithSearchQueryParameter_ReturnsExpectedRecipes()
	{
		// Arrange
		var macRecipeRequest = new CreateRecipeRequest("Macaroni & Cheese", "Cooked macaroni with melted cheese.");
		var pastaBakeRequest = new CreateRecipeRequest("Pasta Bake", "Cooked Penne pasta with cheese and tomato sauce.");

		// Act
		var macRecipeResponse = await _client.PostAsJsonAsync("/api/recipes", macRecipeRequest);
		var pastaBakeResponse = await _client.PostAsJsonAsync("/api/recipes", pastaBakeRequest);

		// Assert
		Assert.NotNull(macRecipeResponse);
		Assert.NotNull(pastaBakeResponse);
		Assert.Equal(HttpStatusCode.Created, macRecipeResponse.StatusCode);
		Assert.Equal(HttpStatusCode.Created, pastaBakeResponse.StatusCode);

		// Arrange
		var macQueryResponse = await _client.GetFromJsonAsync<List<RecipeResponse>>("/api/recipes?search=mac");

		// Assert
		Assert.NotNull(macQueryResponse);
		Assert.Contains(macQueryResponse, recipe => recipe.Name == "Macaroni & Cheese");
	}
}
