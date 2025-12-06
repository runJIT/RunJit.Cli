using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Update.SwaggerTests
{
    internal static class AddUpdateSwaggerTestsParametersExtension
    {
        internal static void AddUpdateSwaggerTestsParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<UpdateSwaggerTestsParameters>();
        }
    }

    internal sealed record UpdateSwaggerTestsParameters(string SolutionFile,
                                                        string GitRepos,
                                                        string WorkingDirectory,
                                                        string IgnorePackages);
}
