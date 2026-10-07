using System.Text.Json;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.ExternalRecipes;
using RecipePlatform.Api.Services;

namespace RecipePlatform.UnitTests.Tests;

public sealed class ExternalRecipeLookupServiceTests
{
	[Fact]
	public async Task LookupAsync_MapsDetailsTagsAndIngredients()
	{
		var client = new StubMealDbClient(new TheMealDbMealDetails
		{
			Id = "52771",
			Name = "Spicy Arrabiata Penne",
			Category = "Vegetarian",
			Area = "Italian",
			Instructions = "Cook pasta.",
			Tags = "Pasta, Italian, ,Vegetarian ",
			AdditionalProperties = new Dictionary<string, JsonElement>
			{
				["strIngredient1"] = JsonSerializer.SerializeToElement("penne rigate"),
				["strMeasure1"] = JsonSerializer.SerializeToElement("1 pound"),
				["strIngredient2"] = JsonSerializer.SerializeToElement(" tomatoes "),
				["strMeasure2"] = JsonSerializer.SerializeToElement(""),
				["strIngredient3"] = JsonSerializer.SerializeToElement("")
			}
		});
		var service = new ExternalRecipeLookupService(client);

		ExternalRecipeDetails? result = await service.LookupAsync("52771", CancellationToken.None);

		Assert.NotNull(result);
		Assert.Equal("52771", result.ExternalId);
		Assert.Equal(["Pasta", "Italian", "Vegetarian"], result.Tags);
		Assert.Equal(
		[
			new ExternalRecipeIngredient("penne rigate", "1 pound"),
			new ExternalRecipeIngredient("tomatoes", null)
		],
			result.Ingredients);
	}

	private sealed class StubMealDbClient(TheMealDbMealDetails meal) : ITheMealDbClient
	{
		public Task<IReadOnlyList<TheMealDbMeal>> SearchMealsAsync(string name, CancellationToken cancellationToken) =>
			throw new NotSupportedException();

		public Task<TheMealDbMealDetails?> LookupMealAsync(string id, CancellationToken cancellationToken) =>
			Task.FromResult<TheMealDbMealDetails?>(meal);
	}
}
