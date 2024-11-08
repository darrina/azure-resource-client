using System.Text;
using Microsoft.Extensions.Logging;

namespace AzureResourceClient;

public class HttpTraceHandler(ILogger<HttpTraceHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log request
        var sb = new StringBuilder();
        sb.AppendLine("cUrl Request:");
        sb.AppendLine($"curl '{request.RequestUri}' \\");
        foreach (var header in request.Headers)
        {
            sb.AppendLine($"  -H '{header.Key}: {string.Join(", ", header.Value)}' \\");
        }
        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                sb.AppendLine($"  -H '{header.Key}: {string.Join(", ", header.Value)}' \\");
            }
            sb.AppendLine($"  --data-raw '{await request.Content.ReadAsStringAsync()}'");
        }
        logger.LogInformation(sb.ToString());

        // Send request
        var response = await base.SendAsync(request, cancellationToken);

        // Log response
        sb = new StringBuilder();
        sb.AppendLine("cUrl Response:");
        sb.AppendLine($"HTTP/{response.Version} {(int)response.StatusCode} {response.ReasonPhrase}");
        foreach (var header in response.Headers)
        {
            sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
        }
        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            sb.AppendLine(await response.Content.ReadAsStringAsync());
        }
        logger.LogInformation(sb.ToString());

        return response;
    }
}
