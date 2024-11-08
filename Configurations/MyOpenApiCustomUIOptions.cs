using System.Reflection;

using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;

namespace AzureResourceClient.Configurations
{
    public class MyOpenApiCustomUIOptions : DefaultOpenApiCustomUIOptions
    {
        public MyOpenApiCustomUIOptions(Assembly assembly)
            : base(assembly)
        {
        }

        public override string CustomStylesheetPath { get; set; } = "dist.my-custom.css";
        public override string CustomJavaScriptPath { get; set; } = "dist.my-custom.js";

        //<!-- Uncomment if you want to use the external URL. -->
        //public override string CustomStylesheetPath { get; set; } = "https://raw.githubusercontent.com/Azure/azure-functions-openapi-extension/main/samples/Microsoft.Azure.WebJobs.Extensions.OpenApi.FunctionApp.InProc/dist/my-custom.css";
        //public override string CustomJavaScriptPath { get; set; } = "https://raw.githubusercontent.com/Azure/azure-functions-openapi-extension/main/samples/Microsoft.Azure.WebJobs.Extensions.OpenApi.FunctionApp.InProc/dist/my-custom.js";
        //<!-- Uncomment if you want to use the external URL. -->
    }
}
