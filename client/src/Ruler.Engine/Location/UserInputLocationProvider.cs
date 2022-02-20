using System;
using System.Collections.Generic;
using System.IO;

namespace Ruler.Engine.Location
{
    /// <summary>
    ///     An <see cref="ILocationProvider"/> which prompts for user input.
    /// </summary>
    public abstract class UserInputLocationProvider : ILocationProvider
    {
        public abstract IEnumerable<string> Locations { get; }

        /// <summary>
        ///     Retrieves a location with a prompt.
        /// </summary>
        /// <param name="args">Argument one should be the user prompt. Argument two is an optional boolean which, if true, skips checks for <see cref="Locations"/>.</param>
        public virtual string GetLocation(params object?[] args)
        {
            if (args.Length > 1 && (bool) (args[2] ?? false))
                goto NoLocations;
            
            foreach (string path in Locations)
                if (Directory.Exists(path))
                    return path;
            
            NoLocations:
            if (args.Length == 0)
                return Console.ReadLine() ?? "";
            
            Console.WriteLine(args[0]);
            return Console.ReadLine() ?? "";
        }
    }
}