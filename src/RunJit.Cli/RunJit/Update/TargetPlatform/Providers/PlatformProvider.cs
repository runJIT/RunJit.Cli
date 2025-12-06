using System.Collections.Immutable;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Update.TargetPlatform
{
    public static class AddPlatformProviderExtension
    {
        public static void AddPlatformProvider(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<PlatformProvider>();
        }
    }

    public class PlatformProvider
    {
        private readonly ImmutableList<string> _platforms = ImmutableList.Create<string>("win-x86", "win-x64", "win-arm",
                                                                                         "win-arm64", "linux-x64", "linux-arm",
                                                                                         "linux-arm64", "linux-musl-x64", "linux-musl-arm64",
                                                                                         "osx-x64", "osx-arm64", "freebsd-x64");

        public ImmutableList<string> GetSupportedPlatforms()
        {
            return _platforms;
        }
    }
}
