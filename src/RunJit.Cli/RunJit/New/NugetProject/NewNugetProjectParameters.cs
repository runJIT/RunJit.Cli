using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.New.NugetProject
{
    internal static class AddNewNugetProjectParametersExtension
    {
        internal static void AddNewNugetProjectParameters(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<NewNugetProjectParameters>();
        }
    }

    internal sealed record NewNugetProjectParameters(bool StartIde,
                                                     string ProjectName,
                                                     DirectoryInfo TargetDirectoryInfo,
                                                     int TargetFramework);
}
