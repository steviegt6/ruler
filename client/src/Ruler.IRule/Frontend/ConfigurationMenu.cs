using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ruler.Engine.Manifest;
using Spectre.Console;

namespace Ruler.IRule.Frontend
{
    public static class ConfigurationMenu
    {
        public static async Task OpenConfigurationMenu()
        {
            List<string> options = new()
            {
                "Change Launch Delay",
                "Change Update Reviewing",
                "Change Release Branch",
                "Exit Config Menu"
            };

            while (true)
            {
                string cfg = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\nSelect a configuration option:")
                        .PageSize(5)
                        .MoreChoicesText("[grey]Move up/down to see more.[/]")
                        .AddChoices(options)
                );

                if (cfg == "Exit Config Menu")
                    break;

                switch (cfg)
                {
                    case "Change Launch Delay":
                        Program.Config.LaunchDelay = AnsiConsole.Ask<int>("Please enter a new millisecond value:");
                        break;
                    
                    case "Change Update Reviewing":
                        Program.Config.ReviewUpdates = AnsiConsole.Ask<bool>("Please enter a new value (true/false):");
                        break;
                    
                    case "Change Release Branch":
                        HttpResponseMessage resp = await Program.Client.GetAsync(Program.BranchEndpoint + "branches-manifest.json");
                        BranchesManifest? br = JsonConvert.DeserializeObject<BranchesManifest>(
                            await resp.Content.ReadAsStringAsync()
                        );

                        if (br is null)
                            throw new Exception("Could not retrieve branch data from endpoint.");

                        Dictionary<string, string> branches = br.Branches.ToDictionary(
                            branch => branch.Key, branch => branch.Value.Name
                        );

                        string chosenBranch = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Pick a branch to switch to:").AddChoices(branches.Values));

                        Program.Config.Branch = branches.Keys.First(x => branches[x].Equals(chosenBranch));
                        break;
                }
            }

            await WriteConfig();
        }

        public static async Task WriteConfig() =>
            await File.WriteAllTextAsync(Program.ConfigPath, JsonConvert.SerializeObject(Program.Config));
    }
}