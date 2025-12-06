using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;
using RunJit.Cli.Services;

namespace RunJit.Cli.RunJit.Generate.Client
{
    internal static class AddCollectIntegrateIntoSourceSolutionExtension
    {
        internal static void AddCollectIntegrateIntoSourceSolution(this IServiceCollection services)
        {
            services.AddIntegrateIntoSourceSolutionValidator();
            services.AddCollectTillInputCorrect();

            services.AddSingletonIfNotExists<CollectIntegrateIntoSourceSolution>();
        }
    }

    internal sealed class CollectIntegrateIntoSourceSolution(IntegrateIntoSourceSolutionValidator inputValidator,
                                                             ICollectTillInputCorrect collectTillInputCorrect)
    {
        private const string Title = @"Where would you like to create your client:
1. Into your given source solutions {0}
2. Create a new solution just owned for the client

Enter one of the option numbers like '1' or '2'";

        public bool Collect(FileInfo sourceSolution)
        {
            var formattedTitle = string.Format(Title, sourceSolution.FullName);
            var optionResult = collectTillInputCorrect.CollectTillInputIsValid(formattedTitle, inputValidator);

            // Default is integrate into your source solution.
            return int.Parse(optionResult).EqualsTo(1);
        }
    }
}
