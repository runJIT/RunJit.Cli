using System.Diagnostics;
using Extensions.Pack;
using Microsoft.Extensions.DependencyInjection;

namespace RunJit.Cli.Services
{
    internal static class AddConsoleServiceExtension
    {
        internal static void AddConsoleService(this IServiceCollection services)
        {
            services.AddSingletonIfNotExists<ConsoleService>();
        }
    }

    internal sealed class ConsoleService
    {
        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteInfo(string value)
        {
            WriteLine(value);
        }

        public void WriteInput(string value)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine(value);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void WriteSuccess(string value)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLine(value);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public string ReadLine()
        {
            var result = Console.ReadLine();
            Console.WriteLine();

            return result ?? string.Empty;
        }

        public void WriteSample(string value)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(value);
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
        }

        public void WriteError(string value)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLine(value);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void WriteLine(string value)
        {
            Console.WriteLine(value);
            Debug.WriteLine(value);
        }
    }
}
