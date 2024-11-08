using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.ResourceManager.Consumption;
using Azure.Core;
using Azure.ResourceManager;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using AzureResourceClient.SecurityAuthFlows;

namespace AzureResourceClient;

public class ConsumptionList(ILogger<ConsumptionList> logger, IConfiguration configuration)
{
    [Function(nameof(ConsumptionList))]
    [OpenApiOperation(operationId: "Run", tags: ["consumption"], Summary = "List consumption budgets", Description = "This will list all the consumption budgets", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiSecurity("consumption_auth", SecuritySchemeType.OAuth2, Flows = typeof(ConsumptionAuthFlows))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        // Extract the OAuth2 token from the request headers
        if (!req.Headers.TryGetValue("Authorization", out var authHeader) || !authHeader.ToString().StartsWith("Bearer "))
        {
            return new UnauthorizedResult();
        }

        var token = authHeader.ToString().Substring("Bearer ".Length).Trim();

        // Create a TokenCredential using the extracted token
        var jwtTokenCredential = new JwtTokenCredential(token);

        // Authenticate your client using the token credential
        var client = new ArmClient(jwtTokenCredential);

        // Get the collection of this ConsumptionBudgetResource
        var scope = $"/subscriptions/{configuration["SubscriptionId"]}";
        var scopeId = new ResourceIdentifier(scope);

        try
        {
            // Get the consumption budgets
            var consumptionBudgets = client.GetConsumptionBudgets(scopeId);

            await foreach (var item in consumptionBudgets.GetAllAsync())
            {
                // For demo we just print out the id
                logger.LogInformation($"Succeeded on id: {item.Data.Id}");
            }

            return new OkObjectResult(consumptionBudgets);
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex.Message);
        }
    }
}
