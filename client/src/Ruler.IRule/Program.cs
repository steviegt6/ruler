using System;
using System.Threading.Tasks;
using CliFx;
using Spectre.Console;

namespace Ruler.IRule
{
    internal static class Program
    {
        public const string Endpoint = "https://i-rule.tomat.dev/";
        
        internal static async Task<int> Main(string[] args)
        {
            AnsiConsole.MarkupLine($@"
[lightgoldenrod2_1]Ruler.IRule v{typeof(Program).Assembly.GetName().Version}[/]
[lightgoldenrod3]by Tomat[/]
");
            
            return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
        }
    }
}