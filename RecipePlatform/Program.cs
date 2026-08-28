using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RecipePlatform.Api.Data;
using RecipePlatform.Api.Data.DTOs;
using RecipePlatform.Api.Interfaces;
using RecipePlatform.Api.Models;
using RecipePlatform.Api.Services;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
	.ReadFrom.Configuration(context.Configuration)
	.ReadFrom.Services(services)
	.Enrich.FromLogContext()
	.Enrich.WithProperty("Application", "RecipePlatform")
	.WriteTo.Console(new RenderedCompactJsonFormatter()),
	preserveStaticLogger: true);

builder.Services.AddDbContext<RecipeDbContext>(options =>
{
	options.UseNpgsql(
		builder.Configuration.GetConnectionString("Postgres"));
});

var tenantId = builder.Configuration["Authentication:TenantId"];
var audience = builder.Configuration["Authentication:Audience"];

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.Authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
		options.Audience = audience;
		options.MapInboundClaims = false;
		options.TokenValidationParameters.RoleClaimType = "roles";
		options.Events = new JwtBearerEvents
		{
			OnAuthenticationFailed = context =>
			{
				Log.Warning(context.Exception, "Bearer token validation failed.");
				return Task.CompletedTask;
			}
		};
	});

builder.Services.AddAuthorization(options =>
{
	options.AddPolicy("RecipesRead", policy =>
	{
		policy.RequireAuthenticatedUser();
		policy.RequireAssertion(context =>
			context.User.FindFirst("scp")?.Value
				.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Contains("recipes.read", StringComparer.Ordinal) == true);
	});

	options.AddPolicy("RecipesWrite", policy =>
	{
		policy.RequireAuthenticatedUser();
		policy.RequireRole("RecipeWriter");
		policy.RequireAssertion(context =>
			context.User.FindFirst("scp")?.Value
				.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Contains("recipes.write", StringComparer.Ordinal) == true);
	});
});

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
	builder.Services.AddStackExchangeRedisCache(options =>
	{
		options.Configuration = redisConnectionString;
		options.InstanceName = "recipe-platform:";
	});
}

var healthChecks = builder.Services.AddHealthChecks().AddDbContextCheck<RecipeDbContext>();

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
	healthChecks.AddRedis(redisConnectionString, name: "redis");
}

builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<IRecipeService, CachedRecipeService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
	Exception? exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
	Log.Error(exception, "Unhandled request failure. {RequestMethod} {RequestPath} {TraceId}", context.Request.Method, context.Request.Path, context.TraceIdentifier);
	await Results.Problem(statusCode: StatusCodes.Status500InternalServerError).ExecuteAsync(context);
}));

app.UseSerilogRequestLogging(options =>
{
	options.GetLevel = (_, elapsed, exception) => exception is not null || elapsed > 1_000 ? LogEventLevel.Warning : LogEventLevel.Information;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapGet("/", () => "RecipePlatform API is running");

app.MapGet(
	"/api/recipes",
	async (IRecipeService recipeService, [AsParameters] GetRecipesQuery query, CancellationToken cancellationToken) =>
	{
		var recipes = await recipeService.GetRecipesAsync(query, cancellationToken);
		return Results.Ok(recipes);
	})
	.RequireAuthorization("RecipesRead");

app.MapPost(
	"/api/recipes",
	async (
		CreateRecipeRequest request,
		CancellationToken cancellationToken,
		IRecipeService recipeService) =>
	{
		if (string.IsNullOrWhiteSpace(request.Name))
		{
			Log.Warning("Recipe creation rejected because the name was blank.");
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
	})
	.RequireAuthorization("RecipesWrite");

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
	})
	.RequireAuthorization("RecipesRead");

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
	})
	.RequireAuthorization("RecipesWrite");

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
	})
	.RequireAuthorization("RecipesWrite");

app.Run();
