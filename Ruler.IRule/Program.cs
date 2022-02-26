using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace Ruler.IRule
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                .Build();

            AnsiConsole.MarkupLine($@"
[red1 bold]Ruler.IRule[/] [white]v{typeof(Program).Assembly.GetName().Version}[/]
[indianred1]by Tomat[/]
[gray]Get support on [#5865F2]Discord[/]: [blue]https://discord.gg/Y8bvvqyFQw[/]
Report issues on GitHub: [blue]https://github.com/Steviegt6/ruler[/][/]
");
            var menu = new Menu(configuration);
            new Thread(() => 
                menu.ExecuteAsync()
                .Wait())
                .Start();
        }
    }
}