using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Ruler.Engine.Platform;
using Ruler.IRule.Frontend;
using Spectre.Console;

namespace Ruler.IRule.Backend
{
    public static class UserInputHandler
    {
        public static async Task HandleInput(bool waited)
        {
            if (waited)
            {
                // TODO: Play the game lol
                return;
            }
            
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Enter:
                    // TODO: Play the game lol
                    break;
                
                default:
                    List<string> stuffToDo = new()
                    {
                        "Manually Choose Version",
                        "Open Config Menu",
                        "View Changelog",
                        "Open Save Directory",
                        "Import .zip File",
                        "Exit Program"
                    };
                    
                    string chosen = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("\nWhat would you like to do?").AddChoices(stuffToDo));

                    switch (chosen)
                    {
                        case "Manually Choose Version":
                            // TODO: this
                            return;
                        
                        case "Open Config Menu":
                            await ConfigurationMenu.OpenConfigurationMenu();
                            return;
                        
                        case "View Changelog":
                            DisplayChangelog();
                            return;
                        
                        case "Open Save Directory":
                            string path = Path.Combine(DesktopLocationProvider.GetDesktopProvider().GetLocation(), "I.RULE");

                            if (OperatingSystem.IsWindows())
                                Process.Start("explorer.exe", path);
                            else
                                Process.Start(path);
                            return;
                        
                        case "Import .zip File":
                            ZipImporter.ImportZip(AnsiConsole.Ask<string>("Please enter the path to the .zip file:"));
                            return;
                        
                        case "Exit Program":
                            return;
                    }
                    return;
            }
        }

        private static void DisplayChangelog()
        {
            AnsiConsole.MarkupLine($"Thanks for using [red1 bold]Ruler.IRule[/] {typeof(Program).Assembly.GetName().Version}!" +
                                   "\n" +
                                   "\nThis version contains an almost entirely rewritten codebase." +
                                   "\nThis rewrite brought along some improvements and additions:" +
                                   "\n * The user input prompt is now much more responsive." +
                                   "\n * Downloads are more reliable and considerably faster." +
                                   "\n * You are now able to access other branches, and can download from them." +
                                   "\n * You can configure which branch to download from." +
                                   "\n * Ruler stays open when I.RULE is running, in case anything is printed to the console." +
                                   "\n * The context menu and version selection system have been entirely reworked." +
                                   "\n * You can now import .zip files as versions.");
            
            AnsiConsole.MarkupLine("Press [u]<ENTER>[/] to exit.");

            Guh:
            ConsoleKey key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.Enter:
                    return;

                default:
                    goto Guh;
            }
        }
    }
}