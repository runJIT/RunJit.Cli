using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;
using RunJit.Cli.Services.Endpoints;

namespace RunJit.Cli.Generate.DotNetTool
{
    internal static class AddCommandMethodBuilderBuilderExtension
    {
        internal static void AddCommandMethodBuilderBuilder(this IServiceCollection services)
        {
            services.AddBuiltInTypeTableService();

            services.AddSingletonIfNotExists<CommandMethodBuilder>();
        }
    }

    // What we are create here:
    // - We create for each http method / controller method the method which the clients need to call this endpoint
    // - Including all url parameters, payloads and relative url.
    // Sample:
    //
    // public Task<IEnumerable<AdminPrivilege>> GetAdminPrivilegeListAsync(int projectId, bool useCache = true)
    // {
    //     return _httpCallHandler.CallAsync<IEnumerable<AdminPrivilege>>(HttpMethod.Get, $"admin/project/{projectId}/privilege/list?useCache={useCache}", null);
    // }
    internal sealed class CommandMethodBuilder
    {
        private const string CallInfosTemplate = """
                                                 // 2. Convert into target type
                                                 $callInfosExists$var callInfos = parameters.Json.FromJsonStringAs<Dictionary<string, object?>>();

                                                 // 3. Setup all needed variables
                                                 """;

        private readonly string[] _httpActionWithPayloads = { "Post", "Patch", "Put" };

        // ToDo: Question how to communicate obsolete methods ?
        private readonly string _methodTemplate = """
                                                  // 1. create client
                                                  var httpCallHandler = $dotNetToolName$HttpClientFactory.CreateFrom(parameters.Token);

                                                  $callInfos$
                                                  $variables$

                                                  // Get the response as string -> only OK responses are provided here, error will be handled as exception
                                                  var stringResponse = await httpCallHandler.CallAsync(HttpMethod.$httpMethod$, $$url$, $payloadAsJson$, $cancellationToken$).ConfigureAwait(false);

                                                  // 3. Write the formatted string to the output
                                                  await outputService.WriteAsync(stringResponse, parameters.Output, parameters.Format, cancellationToken).ConfigureAwait(false);
                                                  """;

        internal string BuildFor(EndpointInfo endpointInfo,
                                 DotNetToolInfos dotNetToolInfos)
        {
            // Any call to a http instance is never sync like like without a Task - we never do blocking API calls !!
            var normalizedReturnType = endpointInfo.ResponseType.Original.EqualsTo("Task<ActionResult>") || endpointInfo.ResponseType.Original.EqualsTo("Task<IActionResult>") || endpointInfo.ResponseType.Original.EqualsTo("ActionResult") || endpointInfo.ResponseType.Original.EqualsTo("IActionResult") || endpointInfo.ResponseType.Original.EqualsTo("void")
                                           ? "Task"
                                           : endpointInfo.ResponseType.Original;

            normalizedReturnType = normalizedReturnType.StartWith("Task").IsFalse() ? $"Task<{normalizedReturnType}>" : normalizedReturnType;

            var httpCallReturnType = endpointInfo.ResponseType.Normalized.Contains("ActionResult") || endpointInfo.ResponseType.Normalized.ToLowerInvariant().EqualsTo("void") ? string.Empty :
                                     normalizedReturnType.EqualsTo("Task") ? string.Empty : normalizedReturnType.Replace("Task<", "<");

            var payload = _httpActionWithPayloads.Contains(endpointInfo.HttpAction) ? ", payload" : string.Empty;
            var httpClientCall = _httpActionWithPayloads.Contains(endpointInfo.HttpAction) ? $"{endpointInfo.HttpAction}AsJson" : endpointInfo.HttpAction;

            // ToDo: detect which parameter is the payload !!
            var fromBody = endpointInfo.Parameters.FirstOrDefault(p => p.Attributes.Any(a => a.Name.EqualsTo("FromBody")));

            var parameterBody = endpointInfo.Parameters.FirstOrDefault(p => endpointInfo.RequestType!.Type.EqualsTo(p.Type));

            var payloadParameter = fromBody.IsNotNull() ? fromBody.Name : parameterBody?.Name;
            payloadParameter = payloadParameter.IsNullOrWhiteSpace() ? "null" : payloadParameter;

            // Any call to a http instance is never sync like like without a Task - we never do blocking API calls !!
            var methodName = endpointInfo.SwaggerOperationId.IsNullOrWhiteSpace() ? endpointInfo.Name : endpointInfo.SwaggerOperationId.FirstCharToUpper();

            // Check for Async post fix ! very important in C# / Net environment -> No method without async at client level because we invoke http calls async !
            methodName = methodName.EndsWith("Async", StringComparison.Ordinal) ? methodName : $"{methodName}Async";

            var parameters = endpointInfo.Parameters.Any()
                                 ? endpointInfo.Parameters.Select(p =>
                                                                  {
                                                                      var defaultValue = p.IsOptional ? $" = {p.DefaultValue}" : string.Empty;
                                                                      var nullable = p.DefaultValue.EqualsTo("null") ? p.Type.EndWith("?") ? string.Empty : "?" : string.Empty;
                                                                      var parameter = $"{p.Type}{nullable} {p.Name}{defaultValue}";
                                                                      parameter = parameter.Replace("IFormFile", nameof(InMemoryFileAsStream));

                                                                      return parameter;
                                                                  }).Flatten(", ")
                                 : string.Empty;

            var attributes = endpointInfo.ObsoleteInfo.IsNotNull() ? $"""[Obsolete("{endpointInfo.ObsoleteInfo.Info}")]""" : string.Empty;
            var cancellationTokenParameter = endpointInfo.Parameters.FirstOrDefault(p => p.Type.EqualsTo(nameof(CancellationToken)));
            var cancellationToken = cancellationTokenParameter.IsNotNull() ? cancellationTokenParameter.Name : $"{nameof(CancellationToken)}.{nameof(CancellationToken.None)}";

            var parameterAsVariables = endpointInfo.Parameters.Where(p => p.Name.Contains("cancellation", StringComparison.OrdinalIgnoreCase).IsFalse())
                                                   .Select(p =>
                                                           {
                                                               if (p.IsOptional)
                                                               {
                                                                   return $"var {p.Name} = DictionaryExtensions.GetValueOrDefault(callInfos,\"{p.Name}\"); // Optional";
                                                               }

                                                               return $$"""var {{p.Name}} = callInfos["{{p.Name}}"]; // Mandatory""";
                                                           })
                                                   .ToFlattenString(Environment.NewLine);

            // var callInfos = endpointInfo.Parameters.IsEmpty() ? string.Empty : callInfosTemplate;
            var callInfos = CallInfosTemplate;
            var ignoreCallInfos = parameterAsVariables.IsNotNullOrWhiteSpace() ? string.Empty : "// ";

            var method = _methodTemplate.Replace("$returnType$", normalizedReturnType)
                                        .Replace("$name$", methodName)
                                        .Replace("$parameters$", parameters)
                                        .Replace("$url$", $@"""{endpointInfo.RelativeUrl}""")
                                        .Replace("$httpMethod$", endpointInfo.HttpAction)
                                        .Replace(", $payload$", payload)
                                        .Replace("$returnTypeNoTask$", httpCallReturnType)
                                        .Replace("$payloadAsJson$", payloadParameter)
                                        .Replace("$httpClientCall$", httpClientCall)
                                        .Replace("$attributes$", attributes)
                                        .Replace("$cancellationToken$", cancellationToken)
                                        .Replace("$variables$", parameterAsVariables)
                                        .Replace("$callInfos$", callInfos)
                                        .Replace("$dotNetToolName$", dotNetToolInfos.NormalizedName.FirstCharToLower())
                                        .Replace("$callInfosExists$", ignoreCallInfos);

            return method;
        }
    }
}
