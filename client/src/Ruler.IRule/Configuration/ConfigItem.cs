using System;
using Spectre.Console;

namespace Ruler.IRule.Configuration
{
    public class ConfigItem<T> : IConfigItem<T>
    {
        object? IConfigItem.Value
        {
            get => Value;
            set => Value = (T?) value;
        }

        public Type Type { get; } = typeof(T);
        
        public string ValidationError { get; }

        public T? Value
        {
            get => Getter();
            set => Setter(value);
        }

        private readonly Func<T?> Getter;
        private readonly Action<T?> Setter;
        
        public ConfigItem(Func<T?> getter, Action<T?> setter, string validationError)
        {
            Getter = getter;
            Setter = setter;
            ValidationError = validationError;
        }

        public TextPrompt<T> MakeTextPrompt(string prompt, string style, Func<T, bool> validate) =>
            new TextPrompt<T>(prompt)
                .PromptStyle(style)
                .ValidationErrorMessage(ValidationError)
                .Validate(validate);
        
        object? IConfigItem.GetPromptResult(
            string prompt,
            string style,
            Func<object, bool> validate
        ) => GetPromptResult(prompt, style,validate);

        public T? GetPromptResult(
            string prompt,
            string style,
            Func<object, bool> validate
        ) => (T?) MakeTextPrompt(prompt, style, arg => validate(arg!)).Show(AnsiConsole.Console);
    }

    public interface IConfigItem<T> : IConfigItem
    {
        new T? Value { get; set; }

        TextPrompt<T> MakeTextPrompt(string prompt, string style, Func<T, bool> validate);

        new T? GetPromptResult(string prompt, string style, Func<object, bool> validate);
    }
    
    public interface IConfigItem
    {
        object? Value { get; set; }

        object? GetPromptResult(string prompt, string style, Func<object, bool> validate);
        
        Type Type { get; }

        string ValidationError { get; }
    }
}