using RecipePlatform.Api.Data.DTOs;

namespace RecipePlatform.Api.Interfaces;

public interface IExternalRecipeSearchService
{
	Task<IReadOnlyList<ExternalRecipeSearchResult>> SearchAsync(string name, CancellationToken cancellationToken);
}
