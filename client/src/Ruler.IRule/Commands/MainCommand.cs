using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
            VersionsManifest? vers = JsonConvert.DeserializeObject<VersionsManifest>(
                await versResp.Content.ReadAsStringAsync()
            );

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

                    string sel = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Select an [red]I.RULE[/] version:")
                            .PageSize(7)
                            .MoreChoicesText("[grey]Move up/down to see more.[/]")
                            .AddChoices(choices)
                    );
                    string selecedVersion = vers.Versions.First(x => x.Value.Name.Equals(sel)).Key;

                    await InstallAndPlayVersion(baseDir, selecedVersion);
                    return;
                }
            }

            await InstallAndPlayVersion(baseDir, vers.Latest);
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

        private async Task InstallAndPlayVersion(string dir, string versionName)
        {
            HttpResponseMessage versResp = await Client.GetAsync(Program.Endpoint + "versions/" + versionName + "/manifest.json");
            VersionManifest? vers = JsonConvert.DeserializeObject<VersionManifest>(
                await versResp.Content.ReadAsStringAsync()
            );

            if (vers is null)
                throw new InvalidOperationException("Could not find version: " + versionName);

            string dirPath = Path.Join(dir, versionName);

            Directory.CreateDirectory(dirPath);

            if (new DirectoryInfo(dirPath).EnumerateFiles().Any(x => x.Extension.Equals(".exe")))
            {
                StartGame(dirPath);
                return;
            }

            AnsiConsole.MarkupLine($"\nSelected version: [b]{vers.Name}[/] - {vers.Description}\n");

            await AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn()
                ).StartAsync(async x =>
                {
                    var task = x.AddTask("Downloading release.zip", new ProgressTaskSettings
                    {
                        AutoStart = false
                    });

                    await DownloadRelease(dirPath, versionName, task);
                });

            AnsiConsole.MarkupLine("Unzipping (extracting) [u]release.zip[/]...");
            UnzipRelease(dirPath);
            
            // Actually play the game lol.
            StartGame(dirPath);
        }

        private async Task DownloadRelease(string dir, string version, ProgressTask task)
        {
            // https://stackoverflow.com/a/56091135
            const int bufferSize = 1024;

            AnsiConsole.MarkupLine("[i]Preparing to download...[/]");
            
            using HttpResponseMessage resp = await Client.GetAsync(Program.Endpoint + "versions/" + version + "/release.zip");
            task.MaxValue(resp.Content.Headers.ContentLength ?? 0);
            task.StartTask();

            AnsiConsole.MarkupLine(
                $"Downloading [u]release.zip[/] according to version [u]{version}[/]! ({task.MaxValue} total bytes)"
            );

            await using Stream cStream = await resp.Content.ReadAsStreamAsync();
            await using FileStream fStream = new(
                Path.Combine(dir, "release.zip"),
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize,
                true
            );

            byte[] buf = new byte[bufferSize];
            while (true)
            {
                int read = await cStream.ReadAsync(buf);

                if (read == 0)
                    break;
                
                task.Increment(read);

                await fStream.WriteAsync(buf.AsMemory(0, read));
            }
        }

        private void UnzipRelease(string dirPath)
        {
            ZipFile.ExtractToDirectory(Path.Combine(dirPath, "release.zip"), dirPath);
            File.Delete(Path.Combine(dirPath, "release.zip"));
        }

        // TODO: Very Windows-focused. If we ever support non-Windows targets, I'll have to update this.
        private static void StartGame(string directory)
        {
            DirectoryInfo dir = new(directory);

            foreach (FileInfo info in dir.EnumerateFiles())
            {
                if (info.Extension != ".exe")
                    continue;

                Process.Start(info.FullName);
                return;
            }

            throw new InvalidOperationException(
                "Attempted to launch game, but no executable was found at directory: " + directory
            );
        }
    }
}