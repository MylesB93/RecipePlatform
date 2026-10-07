using RecipePlatform.Api.Data.DTOs;

namespace RecipePlatform.Api.Interfaces;

public interface IExternalRecipeLookupService
{
	Task<ExternalRecipeDetails?> LookupAsync(string externalId, CancellationToken cancellationToken);
}
