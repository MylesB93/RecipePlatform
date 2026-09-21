using System.Net;
using System.Text;
using RecipePlatform.Api.ExternalRecipes;

namespace RecipePlatform.UnitTests.Tests;

public sealed class TheMealDbClientTests
{
	[Fact]
	public async Task SearchMealsAsync_WhenMealDbReturnsMeals_ReturnsMappedMeals()
	{
		var handler = new StubHttpMessageHandler("""
			{"meals":[{"idMeal":"52771","strMeal":"Spicy Arrabiata Penne","strMealThumb":"https://example.test/meal.jpg"}]}
			""");
		var client = new TheMealDbClient(new HttpClient(handler)
		{
			BaseAddress = new Uri("https://www.themealdb.com/api/json/v1/1/")
		});

		IReadOnlyList<TheMealDbMeal> meals = await client.SearchMealsAsync("Spicy Arrabiata", CancellationToken.None);

		TheMealDbMeal meal = Assert.Single(meals);
		Assert.Equal("52771", meal.Id);
		Assert.Equal("Spicy Arrabiata Penne", meal.Name);
		Assert.Equal("https://example.test/meal.jpg", meal.ThumbnailUrl);
		Assert.Equal("/api/json/v1/1/search.php?s=Spicy%20Arrabiata", handler.RequestUri!.PathAndQuery);
	}

	private sealed class StubHttpMessageHandler(string content) : HttpMessageHandler
	{
		public Uri? RequestUri { get; private set; }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			RequestUri = request.RequestUri;
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(content, Encoding.UTF8, "application/json")
			});
		}
	}
}
