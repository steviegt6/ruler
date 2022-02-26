using Ionic.Zip;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ruler.IRule
{
    public class Menu
    {
        IConfiguration _configuration;
        GitClient _client = new GitClient();
        Bash _bash = new Bash();

        public Menu(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private enum Options
        {
            // General
            Exit,
            // Menu
            IRuler,
            Settings,
            Changelog,
            Import,
            // Game
            Download,
            Launch
        }
        public async Task ExecuteAsync()
        {
            var cd = Directory.GetCurrentDirectory()
                .Replace(@"\", "/");
            var options = new List<Options>
            {
                Options.IRuler,
                Options.Settings,
                Options.Changelog,
                Options.Import,
                Options.Exit
            };
            var menu = _bash.Prompt(options, "Menu");
            if (menu == Options.IRuler)
            {
                options = new List<Options> { Options.Launch, Options.Download, Options.Exit };
                var iRuler = _bash.Prompt(options);
                if (iRuler == Options.Launch)
                {
                    var version = _bash.Prompt(
                        Directory.GetDirectories(cd)
                        .Select(x => x[(x.LastIndexOf('/') + 1)..]), "Version");
                    StartProcess($"{cd}/{version}/");
                }
                else if (iRuler == Options.Download)
                {
                    var downloadable = await _client.GetVersionsAsync(
                                $"{_configuration.GetSection("git:Endpoint").Value}{_configuration.GetSection("git:branch").Value}/versions-manifest.json"
                    );
                    var downloadVersion = _bash.Prompt(downloadable)
                        .Replace(" ", "-")
                        .ToLower();
                    Directory.CreateDirectory($"{cd}/{downloadVersion}");
                    var zip = await _bash.ProgressAsync(
                        _client.Download,
                        $"{_configuration.GetSection("git:Endpoint").Value}{_configuration.GetSection("git:Branch").Value}/versions/{downloadVersion}/release.zip"
                    );
                    UnZipAll(zip, $"{cd}/{downloadVersion}");
                    StartProcess($"{cd}/{downloadVersion}/");
                }
            }
            else if (menu == Options.Settings)
            {
            }
            else if (menu == Options.Changelog)
            {
                _bash.Write(
                        File.ReadAllText(
                            Directory.GetCurrentDirectory() + "/resources/changelog/ansi.txt"
                        )
                    );
            }
            else if (menu == Options.Import)
            {
                var filePath = _bash.Ask<string>("Please enter path to file");
                Directory.CreateDirectory($"{filePath[..filePath.LastIndexOf(".zip")]}");
                ZipFile.Read(filePath).ExtractAll($"{filePath[..filePath.LastIndexOf(".zip")]}");
            }
            else Environment.Exit(0);




            //switch (_bash.Prompt(options, "Menu"))
            //{
            //    case Options.IRuler:
            //        options = new List<Options> { Options.Launch, Options.Download };
            //        switch (_bash.Prompt(options))
            //        {
            //            case Options.Launch:
            //                var launchVersion = _bash.Prompt(Directory.GetDirectories(cd), "Version");
            //                StartProcess($"{cd}/{launchVersion}");
            //                break;
            //            case Options.Download:
            //                var downloadable = await _client.GetVersionsAsync(
            //                    $"{_configuration.GetSection("git:Endpoint").Value}{_configuration.GetSection("git:branch").Value}/versions-manifest.json"
            //                );
            //                var downloadVersion = _bash.Prompt(downloadable)
            //                    .Replace(" ", "-")
            //                    .ToLower();
            //                Directory.CreateDirectory($"{cd}/{downloadVersion}");
            //                var url = _configuration.GetSection("git:Endpoint").Value +
            //                    _configuration.GetSection("git:Branch").Value +
            //                    $"/versions/{downloadVersion}/release.zip";

            //                var zip = await _bash.ProgressAsync(_client.Download, url);
            //                UnZipAll(zip, $"{cd}/{downloadVersion}");
            //                StartProcess($"{cd}/{downloadVersion}");
            //                break;
            //        }
            //        break;
            //    case Options.Settings:
            //        break;
            //    case Options.Changelog:
            //        _bash.Write(
            //            File.ReadAllText(
            //                Directory.GetCurrentDirectory() + "/resources/changelog/ansi.txt"
            //            )
            //        );
            //        break;
            //    case Options.Import:
            //        var filePath = _bash.Ask<string>("Please enter path to file");
            //        var path = filePath[filePath.LastIndexOf('/')..];
            //        var filename = filePath[filePath.LastIndexOf('/')..filePath.LastIndexOf(".zip")];
            //        Directory.CreateDirectory($"{path}/{filename}");
            //        ZipFile.Read(filePath).ExtractAll($"{path}/{filename}");
            //        break;
            //    case Options.Exit:
            //        Environment.Exit(0);
            //        break;
            //    default: break;
            //}
        }


        public void UnZipAll(Stream zip, string directory)
        {
            zip.Seek(0, SeekOrigin.Begin);
            ZipFile.Read(zip)
                .ExtractAll(directory);
        }
        public IEnumerable<Process> StartProcess(string directory)
        {
            var started = new List<Process>();
            var executables = Directory.GetFiles(directory, "*.exe");
            foreach (var exe in executables)
                started.Add(Process.Start($"{exe}"));
            return started;
        }
    }
}