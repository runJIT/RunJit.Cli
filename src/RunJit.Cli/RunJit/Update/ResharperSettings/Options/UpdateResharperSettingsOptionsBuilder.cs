﻿using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Update.ResharperSettings
{
    public static class AddUpdateResharperSettingsOptionsBuilderExtension
    {
        public static void AddUpdateResharperSettingsOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IUpdateResharperSettingsOptionsBuilder, UpdateResharperSettingsOptionsBuilder>();
        }
    }

    internal interface IUpdateResharperSettingsOptionsBuilder
    {
        IEnumerable<Option> Build();
    }

    internal class UpdateResharperSettingsOptionsBuilder : IUpdateResharperSettingsOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return GitRepos();
            yield return SolutionFile();
            yield return WorkingDirectory();
        }

        public Option GitRepos()
        {
            return new Option(new[] { "--git-repos", "-gr" }, "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'")
            {
                Required = false,
                Argument = new Argument<string>("gtiRepos")
                {
                    Description = "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'"
                }
            };
        }

        public Option SolutionFile()
        {
            return new Option(new[] { "--solution", "-s" }, "The solution file which should be updated")
            {
                Required = false,
                Argument = new Argument<string>("solution")
                {
                    Description = "The solution file which should be updated"
                }
            };
        }

        public Option WorkingDirectory()
        {
            return new Option(new[] { "--working-directory", "-wd" }, "The working directory in which all operation should be executed")
            {
                Required = false,
                Argument = new Argument<string>("solutionFile")
                {
                    Description = "The working directory in which all operation should be executed"
                }
            };
        }
    }
}
