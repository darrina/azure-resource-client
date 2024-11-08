using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace AzureResourceClient;

public class AuthTokenProxy(ILogger<AuthTokenProxy> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
{
    [Function(nameof(AuthTokenProxy))]
    [OpenApiOperation(operationId: "Run", tags: ["auth"], Summary = "Proxy auth token request", Description = "Proxies a request to Microsoft login token endpoint", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, ["get", "post"], Route = "auth/token")] HttpRequest req
    )
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        // Create a new HttpRequestMessage
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var requestMessage = new HttpRequestMessage(new HttpMethod(req.Method), configuration["TokenUrl"])
        {
            Content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded")
        };

        // Copy all headers except the Origin header
        req.Headers.Remove("origin");
        req.Headers.Remove("host");
        foreach (var header in req.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }

        // Create a new HttpClient
        using var httpClient = httpClientFactory.CreateClient("UnsafeClient");
        // using var httpClient = new HttpClient(new HttpClientHandler
        // {
        //     ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        // });

        try
        {
            // Send the request to the target URL
            var responseMessage = await httpClient.SendAsync(requestMessage);
        
            // Copy the status code from the response
            req.HttpContext.Response.StatusCode = (int)responseMessage.StatusCode;

            // Copy all headers from the response
            req.HttpContext.Response.Headers.Clear();
            foreach (var header in responseMessage.Headers)
            {
                req.HttpContext.Response.Headers.Append(header.Key, header.Value.ToArray());
            }
            
            // Read the response content
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            
            // Return the response to the client
            return new ContentResult { Content = responseContent }; 
        }
        catch (HttpRequestException ex)
        {
            var e = ex as Exception;
            var errorMessage = e.Message;
            while (e.InnerException != null)
            {
                e = e.InnerException;
                errorMessage += $"\n{e.Message}";
            }
            logger.LogError(errorMessage);

            return new ContentResult { Content = errorMessage, StatusCode = (int)HttpStatusCode.InternalServerError };
        }
    }
}
