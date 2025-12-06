using System.IO.Compression;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Api.Client;
using RunJit.Api.Client.Api.Clients.V1;
using RunJit.Cli.Auth0;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddTemplateExtractorExtension
    {
        internal static void AddTemplateExtractor(this IServiceCollection services,
                                                  IConfiguration configuration)
        {
            services.AddSingletonIfNotExists<ITemplateExtractor, TemplateExtractor>();
            services.AddRunJitApiClientFactory(configuration);
            services.AddMediator();
        }
    }

    internal interface ITemplateExtractor
    {
        Task ExtractToAsync(DirectoryInfo directoryInfo,
                            ClientParameters clientGenParameters);
    }

    internal sealed class TemplateExtractor() : ITemplateExtractor
    {
        public Task ExtractToAsync(DirectoryInfo directoryInfo,
                                   ClientParameters clientGenParameters)
        {
            // Normal mode !!
            // To avoid api start and more
            //var auth = await mediator.SendAsync(new GetTokenByStorageCache()).ConfigureAwait(false);
            //var httpClient = httpClientFactory.CreateClient();
            //httpClient.BaseAddress = new Uri(runJitApiClientSettings.BaseAddress);

            //// httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(auth.TokenType, auth.Token);
            //var rRunJitApiClient = runJitApiClientFactory.CreateFrom(httpClient);

            //var generateClientRequest = new GenerateClientRequest()
            //                            {
            //                                NetVersion = 9
            //                            };

            //var generateClientResponse = await rRunJitApiClient.Clients.V1.GenerateClientAsync(generateClientRequest).ConfigureAwait(false);

            using var clientEmbedded = typeof(TemplateExtractor).Assembly.GetEmbeddedFileAsStream("RunJit.Generate.Client.Templates.client.zip");
            using var zipArchive = new ZipArchive(clientEmbedded);
            zipArchive.ExtractToDirectory(directoryInfo.FullName, true);

            return Task.CompletedTask;
        }
    }
}
