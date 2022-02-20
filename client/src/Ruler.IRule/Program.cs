using System;
using System.Threading.Tasks;
using CliFx;

namespace Ruler.IRule
{
    internal static class Program
    {
        public const string Endpoint = "https://i-rule.tomat.dev/";
        
        internal static async Task<int> Main(string[] args)
        {
            Console.WriteLine($@"
Ruler.IRule v{typeof(Program).Assembly.GetName().Version}
by Tomat
");
            
            return await new CliApplicationBuilder().AddCommandsFromThisAssembly().Build().RunAsync(args);
        }
    }
}