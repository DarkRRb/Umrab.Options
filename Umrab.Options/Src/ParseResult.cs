using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Umrab.Options;

public class ParseResult {
    public Command Command { get; }

    public ParseResult? SubCommand { get; private set; }

    private readonly Dictionary<string, object> _options = [];

    private readonly List<object> _arguments = [];

    internal ParseResult(Command command) {
        Command = command;
    }

    internal void SetSubCommand(ParseResult result) {
        SubCommand = result;
    }

    internal bool TryGetValue(string name, [MaybeNullWhen(false)] out object? value) {
        return _options.TryGetValue(name, out value);
    }

    internal void AddOrUpdateOption(string name, object value) {
        _options[name] = value;
    }

    internal void AddArgument(object value) {
        _arguments.Add(value);
    }

    public T GetOption<T>(string name, Func<T> @default) {
        return _options.TryGetValue(name, out object? value) ? (T)value : @default();
    }

    public T GetArgument<T>(int index, Func<T> @default) {
        return index < _arguments.Count ? (T)_arguments[index] : @default();
    }
}