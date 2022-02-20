using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Newtonsoft.Json;
using Ruler.Engine.Location;
using Ruler.Engine.Manifest;
using Ruler.Engine.Platform;
using Spectre.Console;

#pragma warning disable CliFx_SystemConsoleShouldBeAvoided
namespace Ruler.IRule.Commands
{
    [Command]
    public class MainCommand : ICommand
    {
        private bool ThreeSecondsPassed;
        private readonly HttpClient Client = new();
        private readonly ILocationProvider LocationProvider = DesktopLocationProvider.GetDesktopProvider();

        public async ValueTask ExecuteAsync(IConsole console)
        {
            string baseDir = Path.Combine(LocationProvider.GetLocation(), "I.RULE");

            AnsiConsole.MarkupLine("Welcome to [i]Ruler.IRule[/], the I.RULE installation and updater." +
                                                "\nInstallations at: " + baseDir);

            AnsiConsole.MarkupLine("\nWaiting [u]ten seconds[/], if [u]no input[/] is received...:" +
                                   "\n  * Ruler.IRule will launch the latest version of I.RULE." +
                                   "\n    * If the latest version is not installed, Ruler.IRule will install it." +
                                   "\n  * [u]Press <ENTER>[/] to download/launch the latest version." +
                                   "\n    * Press [u]any other key[/] to select an older version to download/launch.");

            await AwaitUserInput();

            HttpResponseMessage versResp = await Client.GetAsync(Program.Endpoint + "versions-manifest.json");
            VersionsManifest? vers =
                JsonConvert.DeserializeObject<VersionsManifest>(await versResp.Content.ReadAsStringAsync());

            if (vers is null)
                throw new InvalidOperationException(
                    "Could not parse JSON received from: " + Program.Endpoint + "versions-manifest.json"
                );

            if (ThreeSecondsPassed)
                AnsiConsole.MarkupLine("\nNo input received, continuing as planned.");
            else
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    AnsiConsole.MarkupLine("\nReceived <ENTER>, continuing as planned.");
                else
                {
                    AnsiConsole.MarkupLine("\nPlease select a version:");
                    string[] choices = vers.Versions.Select(x => x.Value.Name).ToArray();
                    
                    string selecedVersion = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select an [red]I.RULE[/] version:")
                            .PageSize(7)
                            .MoreChoicesText("[grey]Move up/down to see more.[/]")
                            .AddChoices(choices)
                    );
                    
                    InstallAndPlayVersion(selecedVersion);
                    return;
                }
            }

            InstallAndPlayVersion(vers.Latest);
        }

        private async Task AwaitUserInput()
        {
            for (int i = 0; i < 10000 / 50; i++)
            {
                await Task.Delay(50);

                if (!Console.KeyAvailable)
                    continue;

                await Task.CompletedTask;
                return;
            }

            ThreeSecondsPassed = true;
            await Task.CompletedTask;
        }

        private void InstallAndPlayVersion(string versionName)
        {
        }
    }
}