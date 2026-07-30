using Microsoft.EntityFrameworkCore;
using RecipePlatform.Api.Data;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;
using RecipePlatform.Api.Models;
using RecipePlatform.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RecipeDbContext>(options =>
{
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("Postgres"));
});

builder.Services.AddHealthChecks().AddDbContextCheck<RecipeDbContext>();

builder.Services.AddScoped<IRecipeService, RecipeService>();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => "RecipePlatform API is running");

app.MapGet(
	"/api/recipes",
	async (IRecipeService recipeService, [AsParameters] GetRecipesQuery query, CancellationToken cancellationToken) =>
	{
		var recipes = await recipeService.GetRecipesAsync(query, cancellationToken);
		return Results.Ok(recipes);
	});

app.MapPost(
	"/api/recipes",
	async (
		CreateRecipeRequest request,
		CancellationToken cancellationToken,
		IRecipeService recipeService) =>
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return Results.BadRequest(new
			{
				error = "Recipe name is required."
			});
		}

		var recipe = await recipeService.CreateRecipeAsync(request, cancellationToken);

		var recipeDto = new RecipeDto
		{
			Id = recipe.Id,
			Name = recipe.Name,
			Description = recipe.Description
		};

		return Results.Created(
			$"/api/recipes/{recipe.Id}",
			recipeDto);
	});

app.MapGet(
	"/api/recipes/{id:guid}",
	async (
		Guid id,
		CancellationToken cancellationToken,
		IRecipeService recipeService) =>
	{
		var recipe = await recipeService.GetRecipeAsync(id, cancellationToken);

		return recipe is null
			? Results.NotFound()
			: Results.Ok(recipe);
	});

app.MapDelete(
	"/api/recipes/{id:guid}",
	async (
		Guid id,
		CancellationToken cancellationToken,
		IRecipeService recipeService) =>
	{
		var isValidRecipe = await recipeService.DeleteRecipeAsync(id, cancellationToken);

		return isValidRecipe ? 
			Results.NoContent() : 
			Results.NotFound();
	});

app.MapPut("/api/recipes/{id:guid}",
	async (Guid id,
		UpdateRecipeRequest updateRecipeRequest,
		CancellationToken cancellationToken,
		IRecipeService recipeService) =>
	{
		var recipe = await recipeService.UpdateRecipeAsync(updateRecipeRequest, id, cancellationToken);
		if (recipe == null)
		{
			return Results.NotFound();
		}

		return Results.Ok(recipe);
	});

app.Run();