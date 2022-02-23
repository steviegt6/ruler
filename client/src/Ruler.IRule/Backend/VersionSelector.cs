using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ruler.Engine.Manifest;
using Ruler.Engine.Platform;
using Spectre.Console;

namespace Ruler.IRule.Backend
{
    public static class VersionSelector
    {
        public static async Task SelectVersion(bool autoSelect)
        {
            if (!autoSelect)
                AnsiConsole.MarkupLine(
                    "\n[gray]If you know what you're doing, you can change the release branch in the config menu![/]\n"
                );

            string branch = Program.Endpoint + Program.Config.Branch;
            
            HttpResponseMessage resp = await Program.Client.GetAsync(branch + "/versions-manifest.json");
            VersionsManifest? vers = JsonConvert.DeserializeObject<VersionsManifest>(
                await resp.Content.ReadAsStringAsync()
            );
            
            if (vers is null)
                throw new InvalidOperationException(
                    "Could not parse JSON received from: " + branch + "/versions-manifest.json"
                );

            string version;

            if (autoSelect)
                version = vers.Latest;
            else
            {
                IEnumerable<string> choices = vers.Versions.Select(x => x.Value.Name);

                string sel = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an [red1 bold]I.RULE[/] version:")
                        .PageSize(5)
                        .MoreChoicesText("[gray]Scroll up/down for more![/]")
                        .AddChoices(choices)
                );

                version = vers.Versions.First(x => x.Value.Name.Equals(sel)).Key;
            }

            await InstallAndPlayVersion(branch, version, autoSelect);
        }

        public static async Task InstallAndPlayVersion(string endpoint, string version, bool autoSelect)
        {
            HttpResponseMessage resp = await Program.Client.GetAsync(endpoint + "/versions/" + version + "/manifest.json");
            VersionManifest? vers = JsonConvert.DeserializeObject<VersionManifest>(
                await resp.Content.ReadAsStringAsync()
            );
            
            if (vers is null)
                throw new InvalidOperationException("Could not find version: " + version);
            
            string directory = Path.Combine(DesktopLocationProvider.GetDesktopProvider().GetLocation(), "I.RULE", version);

            Directory.CreateDirectory(directory);

            if (new DirectoryInfo(directory).EnumerateFiles().Any(x => x.Extension.Equals(".exe")))
            {
                await GameLauncher.RunGame(directory);
                return;
            }
            
            AnsiConsole.MarkupLine($"\nSelected version: [b]{vers.Name}[/]");

            if (Program.Config.ReviewUpdates && !autoSelect)
            {
                string desc = vers.Description;

                if (desc == "{{ USE_CHANGELOG }}")
                {
                    HttpResponseMessage clResp = await Program.Client.GetAsync(endpoint + "/versions/" + version + "/changelog.txt");
                    desc = await clResp.Content.ReadAsStringAsync();
                }
                
                AnsiConsole.MarkupLine("[b]VERSION OVERVIEW[/]" +
                                       $"\n[u]{vers.Name}[/]" +
                                       "\n" +
                                       $"\n[grey69]{desc}[/]" +
                                       "\n\nPress [u]<ENTER>[/] to confirm this installation. Press [u]<SPACE>[/] to quit," +
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

                    await DownloadRelease(endpoint, directory, version, task);
                });

            AnsiConsole.MarkupLine("Unzipping (extracting) [u]release.zip[/]...");
            ZipImporter.UnzipFile(directory, Path.Combine(directory, "release.zip"));
            
            await GameLauncher.RunGame(directory);
        }
        
        public static async Task DownloadRelease(string endpoint, string directory, string version, ProgressTask task)
        {
            // https://stackoverflow.com/a/56091135
            const int bufferSize = 1024;

            AnsiConsole.MarkupLine("[i]Preparing to download...[/]");
            
            using HttpResponseMessage resp = await Program.Client.GetAsync(endpoint + "/versions/" + version + "/release.zip");
            task.MaxValue(resp.Content.Headers.ContentLength ?? 0);
            task.StartTask();

            AnsiConsole.MarkupLine(
                $"Downloading [u]release.zip[/] according to version [u]{version}[/]! ({task.MaxValue} total bytes)"
            );

            await using Stream cStream = await resp.Content.ReadAsStreamAsync();
            await using FileStream fStream = new(
                Path.Combine(directory, "release.zip"),
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
    }
}