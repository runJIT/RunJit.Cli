//using System.Text;
//using Extensions.Pack;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace RunJit.Cli.Services.Gpt.Docu
//{
//    /// <summary>
//    /// Base class for documentation commands. It uses a message template to format the prompt.
//    /// </summary>
//    internal abstract class AiDocuCommandBase(GptClient gptClient,
//                                              string template)
//    {
//        internal async Task<string> ExecuteAsync(string codeSnippet)
//        {
//            // Format the prompt by inserting the code snippet into the template.
//            var stringBuilder = new StringBuilder();
//            stringBuilder.AppendLine(template);
//            stringBuilder.AppendLine("Here is the next part code i need to be documentated:");
//            stringBuilder.AppendLine();
//            stringBuilder.AppendLine(codeSnippet);

//            var message = stringBuilder.ToString();

//            var result = await gptClient.PostMessageAsync(message);

//            return result;
//        }
//    }

//    public static class AddDocuEndpointCommandExtension
//    {
//        public static void AddDocuEndpointCommand(this IServiceCollection services)
//        {
//            services.AddSingletonIfNotExists<DocuEndpointCommand>();
//        }
//    }

//    /// <summary>
//    /// Documentation command for endpoints.
//    /// </summary>
//    internal class DocuEndpointCommand : AiDocuCommandBase
//    {
//        private const string ChatMessage = """
//                                           We are using C# and Asp.Net. You should support me by document a minimal endpoint.
//                                           We have not to document the c# class we have to create the detailed summary for the endpoint.
//                                           Important i use the docu result in an automated process. Prepare the result in away that
//                                           i can easisly reuse it.

//                                           I will give you a sample how such a docu looks like:

//                                           Retrieves a collection of form configurations available to the authenticated user.

//                                           This endpoint supports optional filtering using the `title` query parameter. If provided, the system returns only those form configurations where the title matches the specified value. The title filter is case-sensitive and expects a non-empty, non-whitespace string.

//                                           If `title` is not provided, no filtering is applied and all configurations are returned.

//                                           ### Query Parameters
//                                           - `title` (optional): Filters the results by form title.
//                                           - Must not be empty (`""`) or whitespace-only (`" "`).
//                                           - Examples of valid titles: `"This is a cool project"`, `"Hello world"`.

//                                           ### Responses
//                                           - `200 OK`: Request successful, returns a list of form configurations.
//                                           - `400 Bad Request`: Invalid query parameter (e.g., `title` is empty or only whitespace).
//                                           - `401 Unauthorized`: Missing or invalid authentication token.
//                                           - `403 Forbidden`: Authenticated user does not have permission to access this endpoint.
//                                           - `404 Not Found`: The route is invalid or does not exist in the current API version.
//                                           - `422 Unprocessable Entity`: This status is reserved for future use with semantically invalid request payloads (typically for POST, PUT, PATCH).
//                                           - `500 Internal Server Error`: A generic server error occurred while processing the request.
//                                           - `503 Service Unavailable`: The server is temporarily unavailable or under maintenance.
//                                           - `504 Gateway Timeout`: The request timed out at the infrastructure level (e.g., AWS or reverse proxy); may return an HTML error page.

//                                           ### Authorization
//                                           This endpoint requires a valid bearer token. The authenticated user must have permission to read form configurations.

//                                           ### Use Cases
//                                           - Listing all available form templates in UI tools.
//                                           - Filtering forms based on project or use-case titles.
//                                           - Preloading forms metadata for editing or preview.

//                                           For best results, ensure all query parameters follow the expected format. Omitting optional filters will result in the full dataset being returned.
//                                           """;

//        public DocuEndpointCommand(GptClient gptClient)
//            : base(gptClient, ChatMessage)
//        {
//        }
//    }

//    public static class AddDocuCommandCommandExtension
//    {
//        public static void AddDocuCommandCommand(this IServiceCollection services)
//        {
//            services.AddSingletonIfNotExists<DocuCommandCommand>();
//        }
//    }

//    /// <summary>
//    /// Documentation command for commands.
//    /// </summary>
//    internal class DocuCommandCommand : AiDocuCommandBase
//    {
//        public DocuCommandCommand(GptClient gptClient)
//            : base(gptClient, "Generate detailed documentation for the command code:\n{0}\n\nExample:\n// This command triggers the order processing...")
//        {
//        }
//    }

//    public static class AddDocuQueryCommandExtension
//    {
//        public static void AddDocuQueryCommand(this IServiceCollection services)
//        {
//            services.AddSingletonIfNotExists<DocuQueryCommand>();
//        }
//    }

//    /// <summary>
//    /// Documentation command for queries.
//    /// </summary>
//    internal class DocuQueryCommand : AiDocuCommandBase
//    {
//        public DocuQueryCommand(GptClient gptClient)
//            : base(gptClient, "Generate detailed documentation for the query code:\n{0}\n\nExample:\n// This query retrieves the customer data based on filters...")
//        {
//        }
//    }

//    public static class AddDocuValidationCommandExtension
//    {
//        public static void AddDocuValidationCommand(this IServiceCollection services)
//        {
//            services.AddSingletonIfNotExists<DocuValidationCommand>();
//        }
//    }

//    /// <summary>
//    /// Documentation command for validations.
//    /// </summary>
//    internal class DocuValidationCommand : AiDocuCommandBase
//    {
//        public DocuValidationCommand(GptClient gptClient)
//            : base(gptClient, "Generate detailed documentation for the validation code:\n{0}\n\nExample:\n// This validation ensures that the input data meets the required criteria...")
//        {
//        }
//    }

//    public static class AddDocuMapperCommandExtension
//    {
//        public static void AddDocuMapperCommand(this IServiceCollection services,
//                                                IConfiguration configuration)
//        {
//            services.AddGptClient(configuration);

//            services.AddSingletonIfNotExists<DocuMapperCommand>();
//        }
//    }

//    /// <summary>
//    /// Documentation command for mappers.
//    /// </summary>
//    internal class DocuMapperCommand : AiDocuCommandBase
//    {
//        public DocuMapperCommand(GptClient gptClient)
//            : base(gptClient, "Generate detailed documentation for the mapper code:\n{0}\n\nExample:\n// This mapper converts the data model into the view model...")
//        {
//        }
//    }

//    public static class AddAiDocuServiceExtension
//    {
//        public static void AddAiDocuService(this IServiceCollection services,
//                                            IConfiguration configuration)
//        {
//            services.AddDocuEndpointCommand();
//            services.AddDocuCommandCommand();
//            services.AddDocuQueryCommand();
//            services.AddDocuValidationCommand();
//            services.AddDocuMapperCommand(configuration);

//            services.AddSingletonIfNotExists<AiDocuService>();
//        }
//    }

//    /// <summary>
//    /// The AiDocuService aggregates all the individual documentation commands and provides a simple facade.
//    /// </summary>
//    internal class AiDocuService(DocuEndpointCommand docuEndpointCommand,
//                                 DocuCommandCommand docuCommandCommand,
//                                 DocuQueryCommand docuQueryCommand,
//                                 DocuValidationCommand docuValidationCommand,
//                                 DocuMapperCommand docuMapperCommand)
//    {
//        internal Task<string> DocuEndpointAsync(string codeSnippet) => docuEndpointCommand.ExecuteAsync(codeSnippet);

//        internal Task<string> DocuCommandAsync(string codeSnippet) => docuCommandCommand.ExecuteAsync(codeSnippet);

//        internal Task<string> DocuQueryAsync(string codeSnippet) => docuQueryCommand.ExecuteAsync(codeSnippet);

//        internal Task<string> DocuValidationAsync(string codeSnippet) => docuValidationCommand.ExecuteAsync(codeSnippet);

//        internal Task<string> DocuMapperAsync(string codeSnippet) => docuMapperCommand.ExecuteAsync(codeSnippet);
//    }
//}


