using System;
using System.Collections.Generic;
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
using Ruler.IRule.Backend;
using Ruler.IRule.Configuration;
using Spectre.Console;

#pragma warning disable CliFx_SystemConsoleShouldBeAvoided
namespace Ruler.IRule.Commands
{
    [Command]
    public class MainCommand : ICommand
    {
        private bool DelayPassed;
        private readonly HttpClient Client = new();
        private readonly ILocationProvider LocationProvider = DesktopLocationProvider.GetDesktopProvider();
        private volatile bool Completed;

        public async ValueTask ExecuteAsync(IConsole console)
        {
            string baseDir = Path.Combine(LocationProvider.GetLocation(), "I.RULE");

            AnsiConsole.MarkupLine("Welcome to [red]Ruler.IRule[/], the I.RULE installation and updater." +
                                   "\nInstallations are at: " + baseDir);

            AnsiConsole.MarkupLine($"\nWaiting [u]{Program.Config.LaunchDelay} milliseconds[/], if [u]no input[/] is received:" +
                                   "\n * Ruler.IRule will launch (and install, if needed) the [u]latest[/] version of I.RULE" +
                                   "\n" +
                                   "\nPress [u]<ENTER>[/] to continue with the latest version." +
                                   "\nPress [u]any[/] button to open a context menu, where you can change settings, select" +
                                   "\nother versions, etc." +
                                   "\n" +
                                   "\n[gray]Don't like the wait time? Configure it in the configuration menu.[/]"
            );

            await AwaitUserInput();

            HttpResponseMessage versResp = await Client.GetAsync(Program.Endpoint + "versions-manifest.json");
            VersionsManifest? vers = JsonConvert.DeserializeObject<VersionsManifest>(
                await versResp.Content.ReadAsStringAsync()
            );

            if (vers is null)
                throw new InvalidOperationException(
                    "Could not parse JSON received from: " + Program.Endpoint + "versions-manifest.json"
                );

            if (DelayPassed)
                AnsiConsole.MarkupLine("\nNo input received, continuing as planned.");
            else
            {
                ConsoleKey key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.Enter:
                        AnsiConsole.MarkupLine("\nReceived <ENTER>, continuing as planned.");
                        break;
                    
                    case ConsoleKey.C:
                    {
                        if (await OpenConfigurationMenu())
                            await PromptVersionSelection(baseDir, vers);
                        return;
                    }
                    
                    default:
                        await PromptVersionSelection(baseDir, vers);
                        return;
                }
            }

            await InstallAndPlayVersion(true, baseDir, vers.Latest);
        }

        private async Task AwaitUserInput()
        {
#pragma warning disable CS4014 // Disabled as synchronous running here is intentional.
            Task.Run(() =>
            {
                while (!Console.KeyAvailable && !DelayPassed)
                {
                }

                Completed = true;
            });

            Task.Run(async () =>
            {
                await Task.Delay(Program.Config.LaunchDelay);

                if (!Completed)
                    DelayPassed = true;

                Completed = true;
            });
#pragma warning restore CS4014
            
            while (!Completed)
            {
            }

            await Task.CompletedTask;
        }

        private async Task PromptVersionSelection(string baseDir, VersionsManifest vers)
        {
            string[] choices = vers.Versions.Select(x => x.Value.Name).ToArray();

            string sel = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("\nSelect an [red]I.RULE[/] version:")
                    .PageSize(7)
                    .MoreChoicesText("[grey]Move up/down to see more.[/]")
                    .AddChoices(choices)
            );
            string selecedVersion = vers.Versions.First(x => x.Value.Name.Equals(sel)).Key;

            await InstallAndPlayVersion(false, baseDir, selecedVersion);
        }

        private async Task InstallAndPlayVersion(bool auto, string dir, string versionName)
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
                await GameLauncher.RunGame(dirPath);
                return;
            }

            AnsiConsole.MarkupLine($"\n\nSelected version: [b]{vers.Name}[/] - {vers.Description}\n");

            if (Program.Config.ReviewUpdates && !auto)
            {
                string desc = vers.Description;

                if (vers.Description == "{{ USE_CHANGELOG }}")
                {
                    HttpResponseMessage clResp = await Client.GetAsync(Program.Endpoint + "versions/" + versionName + "/changelog.txt");
                    desc = await clResp.Content.ReadAsStringAsync();
                }
                
                AnsiConsole.MarkupLine("[b]VERSION OVERVIEW[/]" +
                                       $"\n[u]{vers.Name}[/]" +
                                       "\n" +
                                       $"\n[grey69]{desc}[/]" +
                                       "\n\nPress <ENTER> to confirm this installation. Press <SPACE> to quit," +
                                       "\n[gray]Don't want to see this prompt? Modify \"Update Reviewing\" in the config![/]");
                
                Guh:
                ConsoleKey key = Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.Enter:
                        break;
                
                    case ConsoleKey.Spacebar:
                        return;
                
                    default:
                        goto Guh;
                }
            }
            
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
            await GameLauncher.RunGame(dirPath);
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

        private async Task<bool> OpenConfigurationMenu()
        {
            Dictionary<string, IConfigItem> configItems = new()
            {
                {"Modify Launch Delay", Program.Config.LaunchDelayItem},
                {"Enable/Disable Update Reviewing", Program.Config.ReviewUpdatesItem},
                {"Exit Config", new ConfigItem<int>(() => 1, val => _ = val, "")}
            };

            while (true)
            {
                string cfg = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\nSelect a configuration option:")
                        .PageSize(5)
                        .MoreChoicesText("[grey]Move up/down to see more, if there are any.[/]")
                        .AddChoices(configItems.Keys)
                );

                if (cfg == "Exit Config")
                    break;

                IConfigItem item = configItems[cfg];
                item.Value = item.GetPromptResult(
                    $"Please enter a new value for \"{cfg}\":",
                    "white",
                    _ => true
                );
            }

            await File.WriteAllTextAsync(Program.ConfigPath, JsonConvert.SerializeObject(Program.Config));
            
            AnsiConsole.MarkupLine("Press [u]<SPACE>[/] to exit. Press [u]<ENTER>[/] to select a version to launch.");

            Guh:
            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.Enter:
                    return true;
                
                case ConsoleKey.Spacebar:
                    return false;
                
                default:
                    goto Guh;
            }
        }
    }
}