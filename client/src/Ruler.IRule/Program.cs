using System;
using System.IO;
using System.Threading.Tasks;
using CliFx;
using Newtonsoft.Json;
using Ruler.Engine.Platform;
using Ruler.IRule.Configuration;
using Spectre.Console;

namespace Ruler.IRule
{
    internal static class Program
    {
        public const string Endpoint = "https://i-rule.tomat.dev/";

        public static readonly string ConfigPath = Path.Combine(
            DesktopLocationProvider.GetDesktopProvider().GetLocation(), "I.RULE", "config.json"
        );

        public static RulerConfig Config = null!;

        internal static async Task<int> Main(string[] args)
        {
            AnsiConsole.MarkupLine($@"
[lightgoldenrod2_1]Ruler.IRule v{typeof(Program).Assembly.GetName().Version}[/]
[lightgoldenrod3]by Tomat[/]
Get support in my [#5865F2]Discord server[/]: [blue]https://discord.gg/Y8bvvqyFQw[/]
");

            if (File.Exists(ConfigPath))
                Config = JsonConvert.DeserializeObject<RulerConfig>(await File.ReadAllTextAsync(ConfigPath)) ?? new RulerConfig();
            else
                Config = new RulerConfig();

            return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
        }
    }
}