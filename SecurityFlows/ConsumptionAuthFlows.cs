using System.Text.Json;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.OpenApi.Models;

namespace AzureResourceClient.SecurityAuthFlows
{
    public record Scope(string Uri, string Description);

    public class ConsumptionAuthFlows : OpenApiOAuthSecurityFlows
    {
        public ConsumptionAuthFlows()
        {
            var authorizationUrl = Environment.GetEnvironmentVariable("AuthorizationUrl").ThrowIfNullOrWhiteSpace();
            var tokenUrl = Environment.GetEnvironmentVariable("ProxyTokenUrl").ThrowIfNullOrWhiteSpace();

            var scopesJson = Environment.GetEnvironmentVariable("Scopes").ThrowIfNullOrWhiteSpace();
            var scopes = JsonSerializer.Deserialize<List<Scope>>(scopesJson) ?? [];

            // Implicit = new OpenApiOAuthFlow()
            // {
            //     AuthorizationUrl = new Uri(authorizationUrl),
            //     TokenUrl = new Uri(tokenUrl),
            //     RefreshUrl = new Uri(tokenUrl),
            //     Scopes = scopes.ToDictionary(scope => scope.Uri, scope => scope.Description)
            // };

            ClientCredentials = new OpenApiOAuthFlow()
            {
                AuthorizationUrl = new Uri(authorizationUrl),
                TokenUrl = new Uri(tokenUrl),
                Scopes = scopes.ToDictionary(scope => scope.Uri, scope => scope.Description)
            };
        }
    }
}
