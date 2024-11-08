using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using AzureResourceClient.Configurations;
using AzureResourceClient.SecurityAuthFlows;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using AzureResourceClient;
using Microsoft.Azure.Functions.Worker;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => worker.UseNewtonsoftJson())
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddLogging();
        services.AddTransient<HttpTraceHandler>();
        services.AddHttpClient("UnsafeClient")
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback  = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            })
            .AddHttpMessageHandler<HttpTraceHandler>();
        services.AddSingleton<IOpenApiConfigurationOptions>(_ => new DefaultOpenApiConfigurationOptions()
        {
            Info = new OpenApiInfo()
            {
                Title = "Azure Resource Consumption API",
                Description = "This is a sample on how to use Azure Resource Manager SDK to list consumption budgets",
                Version = "1.0.0"
            },
        });
        services.AddSingleton(Assembly.GetExecutingAssembly());
        services.AddSingleton<IOpenApiCustomUIOptions, MyOpenApiCustomUIOptions>();
        services.AddSingleton<ConsumptionAuthFlows>();
    })
    .Build();

host.Run();