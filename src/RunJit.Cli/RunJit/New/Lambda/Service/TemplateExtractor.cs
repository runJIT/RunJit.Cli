﻿using System.Collections.Immutable;
using System.IO.Compression;
using System.Net.Http.Headers;
using AspNetCore.Simple.Sdk.Mediator;
using Extensions.Pack;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Api.Client;
using RunJit.Cli.Auth0;
using DirectoryInfo = System.IO.DirectoryInfo;
using FileInfo = System.IO.FileInfo;

namespace RunJit.Cli.RunJit.New.Lambda
{
    public static class AddTemplateExtractorExtension
    {
        public static void AddTemplateExtractor(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<TemplateExtractor>();
        }
    }

    internal class TemplateExtractor(IMediator mediator,
                                     IHttpClientFactory httpClientFactory,
                                     IRunJitApiClientFactory runJitApiClientFactory,
                                     RunJitApiClientSettings runJitApiClientSettings)
    {
        public async Task ExtractToAsync(DirectoryInfo directoryInfo)
        {
            var auth = await mediator.SendAsync(new GetTokenByStorageCache()).ConfigureAwait(false);
            var httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(runJitApiClientSettings.BaseAddress);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(auth.TokenType, auth.Token);
            var rRunJitApiClient = runJitApiClientFactory.CreateFrom(httpClient);

            var codeRuleAsFileStream = await rRunJitApiClient.CodeRules.V1.ExportCodeRulesAsync().ConfigureAwait(false);
            using var zipArchive = new ZipArchive(codeRuleAsFileStream.FileStream, ZipArchiveMode.Read);
            zipArchive.ExtractToDirectory(directoryInfo.FullName);
            
            
            //var template = this.GetType().Assembly.GetEmbeddedFileAsStream("RunJit.New.Lambda.Templates.lambda.template");

            //using var zipArchive = new ZipArchive(template);
            //zipArchive.ExtractToDirectory(directoryInfo.FullName, true);
        }
    }
}