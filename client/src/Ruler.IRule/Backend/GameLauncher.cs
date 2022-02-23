using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Ruler.IRule.Backend
{
    /// <summary>
    ///     Handles launching and monitoring I.RULE.
    /// </summary>
    public static class GameLauncher
    {
        // TODO: Very Windows-focused. If we ever support non-Windows targets, I'll have to update this.
        public static async Task RunGame(string directory)
        {
            DirectoryInfo dir = new(directory);

            foreach (FileInfo info in dir.EnumerateFiles())
            {
                if (info.Extension != ".exe")
                    continue;

                Process process = Process.Start(info.FullName);
                
                // Wait for exit instead of immediately exiting.
                await process.WaitForExitAsync();
                return;
            }

            throw new InvalidOperationException(
                "Attempted to launch game, but no executable was found at directory: " + directory
            );
        }
    }
}