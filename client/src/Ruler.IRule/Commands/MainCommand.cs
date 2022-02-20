using System;
using System.Threading.Tasks;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;

#pragma warning disable CliFx_SystemConsoleShouldBeAvoided
namespace Ruler.IRule.Commands
{
    [Command]
    public class MainCommand : ICommand
    {
        private bool ThreeSecondsPassed;
        
        public async ValueTask ExecuteAsync(IConsole console)
        {
            await console.Output.WriteLineAsync("Welcome to Ruler.IRule, the I.RULE installation and updater.");

            await console.Output.WriteLineAsync("\nWaiting ten seconds, if no input is received...:" +
                                                "\n  * Ruler.IRule will launch the latest version of I.RULE." +
                                                "\n    * If the latest version is not installed, Ruler.IRule will install it." +
                                                "\n  * Press <ENTER> to download/launch the latest version." +
                                                "\n    * Press any other key to select an older version to download/launch.");
            
            await AwaitUserInput();

            if (ThreeSecondsPassed)
                await console.Output.WriteLineAsync("\nNo input received, continuing as planned.");
            else
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    await console.Output.WriteLineAsync("\nReceived <ENTER>, continuing as planned.");
                else
                {
                    await console.Output.WriteLineAsync("\nPlease select a version:");
                }
            }
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
    }
}