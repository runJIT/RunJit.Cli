using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions.Pack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Services.Slack
{
    public static class AddSlackSettingsExtension
    {
        public static void AddSlackSettings(this IServiceCollection services,
                                            IConfiguration configuration)
        {
            services.AddSingletonOption<SlackSettings>(configuration);
        }
    }

    public record SlackSettings
    {
        public string Token { get; init; } = string.Empty;

        public SlackChannel PullRequestChannel { get; init; } = new()
                                                                {
                                                                    Id = "C04JMJ7UCHX",
                                                                    Name = "#backend-pullrequests"
                                                                };
    }

    public record SlackChannel
    {
        public string Name { get; init; } = string.Empty;

        public string Id { get; init; } = string.Empty;
    }
}
