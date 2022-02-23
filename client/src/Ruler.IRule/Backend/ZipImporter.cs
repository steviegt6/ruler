using System.IO;
using System.IO.Compression;
using Ruler.Engine.Platform;
using Spectre.Console;

namespace Ruler.IRule.Backend
{
    public static class ZipImporter
    {
        public static void ImportZip(string filePath)
        {
            if (Path.GetExtension(filePath) != ".zip")
            {
                AnsiConsole.MarkupLine("Please ensure the file ends in [u].zip[/]!");
                return;
            }

            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine("Please ensure this file path points to an [u]existing file[/]!");
                return;
            }
            
            AnsiConsole.MarkupLine("Welcome to our .zip importer. This is for importing versions [u]if[/] a server is down." +
                                   "\nThis is not designed for importing [u]custom[/] versions.");

            string folderName = AnsiConsole.Ask<string>("Please enter the folder name (i.e. version-1.x.x)");
            
            UnzipFile(filePath, Path.Combine(DesktopLocationProvider.GetDesktopProvider().GetLocation(), "I.RULE", folderName));
        }
        
        public static void UnzipFile(string file, string directory)
        {
            Directory.CreateDirectory(directory);
            ZipFile.ExtractToDirectory(file, directory);
            File.Delete(file);
        }
    }
}