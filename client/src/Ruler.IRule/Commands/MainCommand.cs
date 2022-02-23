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
using Ruler.IRule.Frontend;
using Spectre.Console;

#pragma warning disable CliFx_SystemConsoleShouldBeAvoided
namespace Ruler.IRule.Commands
{
    [Command]
    public class MainCommand : ICommand
    {
        private bool DelayPassed;
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
                                   "\nPress [u]any other[/] button to open a context menu, where you can change settings, select" +
                                   "\nother versions, etc." +
                                   "\n" +
                                   "\n[gray]Don't like the wait time? Configure it in the configuration menu.[/]"
            );

            await AwaitUserInput();
            await UserInputHandler.HandleInput(DelayPassed);
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
    }
}