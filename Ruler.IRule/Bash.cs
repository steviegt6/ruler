using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ruler.IRule
{
    public class Bash
    {
        public T Ask<T>(string what)
        {
            return AnsiConsole.Ask<T>(what);
        }
        public T Prompt<T>(IEnumerable<T> choices, string title = null!) where T : notnull
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<T>()
                .AddChoices(choices)
                .Title(title)
           );
        }
        public void Write<T>(T message)
        {
            if (message != null)
                AnsiConsole.MarkupLine(message.ToString()!);
            throw new ArgumentNullException(nameof(message));
        }

        public async Task<T> ProgressAsync<T>(
            Func<ProgressTask, string, Task<T>> func, 
            string param2, 
            string taskName = null!, 
            string message = null!
        ) where T : new()
        {
            var ret = new T();
            await AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn()
                ).StartAsync(async x => {

                    var task = x.AddTask(taskName, false);
                    ret = await func(task, param2);
                });
            AnsiConsole.MarkupLine(message);
            return ret;
        }
    }
}