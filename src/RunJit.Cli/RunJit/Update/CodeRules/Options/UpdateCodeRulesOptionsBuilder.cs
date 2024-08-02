﻿using System.CommandLine;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.RunJit.Update.CodeRules
{
    public static class AddUpdateCodeRulesOptionsBuilderExtension
    {
        public static void AddUpdateCodeRulesOptionsBuilder(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<IUpdateCodeRulesOptionsBuilder, UpdateCodeRulesOptionsBuilder>();
        }
    }

    internal interface IUpdateCodeRulesOptionsBuilder
    {
        IEnumerable<Option> Build();
    }

    internal class UpdateCodeRulesOptionsBuilder : IUpdateCodeRulesOptionsBuilder
    {
        public IEnumerable<Option> Build()
        {
            yield return GitRepos();
            yield return SolutionFile();
            yield return WorkingDirectory();
            yield return IgnorePackage();
            yield return BasedOnBranch();
        }

        public Option GitRepos()
        {
            return new Option(new[] { "--git-repos", "-gr" }, "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'")
                   {
                       Required = false,
                       Argument = new Argument<string>("gtiRepos") { Description = "The git repository urls. Sample: 'codecommit::eu-central-1://runjit-dbi' or multiple 'codecommit::eu-central-1://runjit-dbi;codecommit::eu-central-1://runjit-dbi' separated by ';'" }
                   };
        }

        public Option SolutionFile()
        {
            return new Option(new[] { "--solution", "-s" }, "The solution file which should be updated")
                   {
                       Required = false,
                       Argument = new Argument<string>("solution") { Description = "The solution file which should be updated" }
                   };
        }

        public Option WorkingDirectory()
        {
            return new Option(new[] { "--working-directory", "-wd" }, "The working directory in which all operation should be executed")
                   {
                       Required = false,
                       Argument = new Argument<string>("workingDirectory") { Description = "The working directory in which all operation should be executed" }
                   };
        }

        public Option IgnorePackage()
        {
            return new Option(new[] { "--ignore-packages", "-ip" }, "Option to give the package name with it which should not be updated. Sample. EPP or commaseparated multiple. EPP;MySql")
                   {
                       Required = false,
                       Argument = new Argument<string>("ignorePackages") { Description = "Option to give the package name with it which should not be updated. Sample. EPP or commaseparated multiple. EPP;MySql" }
                   };
        }

        public Option BasedOnBranch()
        {
            return new Option(new[] { "--branch", "-b" }, "Give the reference branch for. If you update code rules by using git url to clone please provide the reference branch from")
                   {
                       Required = false,
                       Argument = new Argument<string>("branch") { Description = "Give the reference branch for. If you update code rules by using git url to clone please provide the reference branch from" }
                   };
        }
    }
}
