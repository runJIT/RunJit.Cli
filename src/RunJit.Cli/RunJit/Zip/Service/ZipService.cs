using System.IO.Compression;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.ErrorHandling;
using RunJit.Cli.Services;

namespace RunJit.Cli.RunJit.Zip
{
    internal static class AddZipServiceExtension
    {
        internal static void AddZipService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IZipService, ZipService>();
        }
    }

    internal interface IZipService
    {
        Task ZipAsync(ZipParameters parameters);
    }

    internal sealed class ZipService(ConsoleService consoleService) : IZipService
    {
        public Task ZipAsync(ZipParameters parameters)
        {
            if (parameters.ZipFile.IsNull())
            {
                return Task.CompletedTask;
            }

            if (parameters.ZipFile.Directory.IsNull() || parameters.ZipFile.Directory.NotExists())
            {
                parameters.ZipFile.Directory!.Create();
            }

            try
            {
                ZipFile.CreateFromDirectory(parameters.Directory.FullName, parameters.ZipFile.FullName);
            }
            catch (Exception e)
            {
                throw new RunJitException($"Could not create zip file: {parameters.ZipFile.FullName}.{Environment.NewLine}{e.Message}");
            }

            consoleService.WriteSuccess("Zip-File successfully created.");
            consoleService.WriteSuccess(parameters.ZipFile.FullName);

            return Task.CompletedTask;
        }
    }

    internal sealed record ZipParameters(DirectoryInfo Directory,
                                         FileInfo ZipFile);
}
