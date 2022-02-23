using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CliFx;
using Newtonsoft.Json;
using Ruler.Engine.Platform;
using Ruler.IRule.Configuration;
using Ruler.IRule.Frontend;
using Spectre.Console;

namespace Ruler.IRule
{
    internal static class Program
    {
        public const string Endpoint = "https://raw.githubusercontent.com/Steviegt6/i-rule-storage/";
        public const string BranchEndpoint = Endpoint + "branches/";
        public const string DefaultBranch = "main";

        public static readonly HttpClient Client = new();
        
        public static readonly string ConfigPath = Path.Combine(
            DesktopLocationProvider.GetDesktopProvider().GetLocation(), "I.RULE", "config.json"
        );

        public static RulerConfig Config = null!;

        internal static async Task<int> Main(string[] args)
        {
            try
            {
                AnsiConsole.MarkupLine($@"
[red1 bold]Ruler.IRule[/] [white]v{typeof(Program).Assembly.GetName().Version}[/]
[indianred1]by Tomat[/]
[gray]Get support on [#5865F2]Discord[/]: [blue]https://discord.gg/Y8bvvqyFQw[/]
Report issues on GitHub: [blue]https://github.com/Steviegt6/ruler[/][/]
");

                if (File.Exists(ConfigPath))
                    Config = JsonConvert.DeserializeObject<RulerConfig>(await File.ReadAllTextAsync(ConfigPath)) ?? new RulerConfig();
                else
                    Config = new RulerConfig();

                await ConfigurationMenu.WriteConfig();
                
                return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine("[gray]A [red bold]FATAL[/] exception occurred while attempting a task." +
                                       "\nPlease [u]immediately[/] report this to the developers." +
                                       "\nFile a ticket here: [blue]https://github.com/Steviegt6/ruler[/]" +
                                       "\nFor debugging purposes, an exception stacktrace as been provided:" +
                                       "\n[/]");
                AnsiConsole.WriteException(e);
                
                AnsiConsole.WriteLine("\n\nPlease press any key to exit...");

                // Force console to wait.
                _ = Console.ReadKey(true);

                return -1;
            }
        }
    }
}