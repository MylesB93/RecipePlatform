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
	async (RecipeDbContext dbContext, IRecipeService recipeService, CancellationToken cancellationToken) =>
	{
		var recipes = await recipeService.GetRecipesAsync(cancellationToken);
		return Results.Ok(recipes);
	});

app.MapPost(
	"/api/recipes",
	async (
		CreateRecipeRequest request,
		RecipeDbContext dbContext,
		CancellationToken cancellationToken) =>
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			return Results.BadRequest(new
			{
				error = "Recipe name is required."
			});
		}

		var recipe = new Recipe
		{
			Id = Guid.NewGuid(),
			Name = request.Name.Trim(),
			Description = request.Description?.Trim()
		};

		dbContext.Recipes.Add(recipe);
		await dbContext.SaveChangesAsync(cancellationToken);

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
		RecipeDbContext dbContext,
		CancellationToken cancellationToken) =>
	{
		var recipe = await dbContext.Recipes
			.AsNoTracking()
			.SingleOrDefaultAsync(
				x => x.Id == id,
				cancellationToken);

		var recipeDto = recipe is null
			? null
			: new RecipeDto
			{
				Id = recipe.Id,
				Name = recipe.Name,
				Description = recipe.Description
			};

		return recipe is null
			? Results.NotFound()
			: Results.Ok(recipeDto);
	});

app.MapDelete(
	"/api/recipes/{id:guid}",
	async (
		Guid id,
		RecipeDbContext dbContext,
		CancellationToken cancellationToken) =>
	{
		var recipe = await dbContext.Recipes
			.SingleOrDefaultAsync(
				x => x.Id == id,
				cancellationToken);
		if (recipe is null)
		{
			return Results.NotFound();
		}
		dbContext.Recipes.Remove(recipe);
		await dbContext.SaveChangesAsync(cancellationToken);
		return Results.NoContent();
	});

app.MapPut("/api/recipes/{id:guid}",
	async (Guid id,
		UpdateRecipeRequest updateRecipeRequest,
		RecipeDbContext dbContext,
		CancellationToken cancellationToken) =>
	{
		var recipe = await dbContext.Recipes.SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
		if (recipe == null)
		{
			return Results.NotFound();
		}

		if (!string.IsNullOrWhiteSpace(updateRecipeRequest.Name) && recipe.Name != updateRecipeRequest.Name)
		{
			recipe.Name = updateRecipeRequest.Name;
		}

		if (!string.IsNullOrWhiteSpace(updateRecipeRequest.Description) && recipe.Description != updateRecipeRequest.Description)
		{
			recipe.Description = updateRecipeRequest.Description;
		}
			
		await dbContext.SaveChangesAsync(cancellationToken);
		return Results.Ok(recipe);
	});

app.Run();