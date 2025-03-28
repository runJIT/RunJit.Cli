using $ProjectName$.Api;
using Siemens.AspNet.MinimalApi.Sdk;

var webApi = new ServerlessMinimalWebApi();

webApi.RegisterServices = (service, config) =>
{
    // Domain service registrations
    service.AddApi(config);
};

webApi.MapEndpoints = endpoints =>
{
    // Map api domain endpoints
    endpoints.MapApi();
};

webApi.Run(args);

// This is important that you are able to use
// API test via WebApplicationFactory<Program>
// https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
namespace $ProjectName$
{
    public partial class Program;
}

