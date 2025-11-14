using System;
using System.Collections.Generic;

using Umrab.Options.Exceptions;
using Umrab.Options.Tokenization;

using OptionLooup = System.Collections.Generic.Dictionary<string, Umrab.Options.IOption>.AlternateLookup<System.ReadOnlySpan<char>>;

namespace Umrab.Options;

public class Command(string name = "") {
    public string Name { get; } = name;

    private readonly Dictionary<string, Command> _commands = [];
    private readonly Dictionary<char, Command> _aliasCommands = [];

    private readonly Dictionary<string, IOption> _options = [];
    private readonly Dictionary<char, IOption> _aliasOptions = [];

    private readonly List<IArgument> _arguments = [];

    public Command SubCommand(string name, Action<Command> builder) {
        Command command = new(name);
        builder(command);
        _commands.Add(name, command);
        return this;
    }

    public Command SubCommand(string name, HashSet<char> aliases, Action<Command> builder) {
        Command command = new(name);
        builder(command);
        _commands.Add(name, command);
        foreach (char alias in aliases) {
            _aliasCommands.Add(alias, command);
        }
        return this;
    }

    public Command Option<T>(string name, bool isFlag, Func<ReadOnlySpan<char>, T?, T> converter) {
        Option<T> option = new(name, isFlag, converter);
        _options.Add(name, option);
        return this;
    }

    public Command Option<T>(string name, HashSet<char> aliases, bool isFlag, Func<ReadOnlySpan<char>, T?, T> converter) {
        Option<T> option = new(name, isFlag, converter);
        _options.Add(name, option);
        foreach (char alias in aliases) {
            _aliasOptions.Add(alias, option);
        }
        return this;
    }
    public Command Option<T>(string name, Func<ReadOnlySpan<char>, T?, T> converter) {
        Option<T> option = new(name, false, converter);
        _options.Add(name, option);
        return this;
    }

    public Command Option<T>(string name, HashSet<char> aliases, Func<ReadOnlySpan<char>, T?, T> converter) {
        Option<T> option = new(name, false, converter);
        _options.Add(name, option);
        foreach (char alias in aliases) {
            _aliasOptions.Add(alias, option);
        }
        return this;
    }

    public Command Argument<T>(Func<ReadOnlySpan<char>, T> converter) {
        Argument<T> argument = new(converter);
        _arguments.Add(argument);
        return this;
    }
    public ParseResult Parse(ReadOnlySpan<string> args) => Parse(new Tokenizer(args));

    private ParseResult Parse(Tokenizer tokenizer) {
        OptionLooup options = _options.GetAlternateLookup<ReadOnlySpan<char>>();

        ParseResult result = new(this);

        Token wovToken = default;
        IOption? wovOption = null;
        int argumentIndex = 0;

        while (tokenizer.TryNext(out Token token)) {
            if (wovOption != null) {
                if (tokenizer.EndOfOption || token.Type != TokenType.Argument) {
                    throw new MissingValueException(wovOption, wovToken);
                }

                object? latest = result.TryGetValue(wovOption.Name, out object? value) ? value : wovOption.Default;
                result.AddOrUpdateOption(wovOption.Name, wovOption.Convert(token.Origin, latest));

                wovToken = default;
                wovOption = null;

                continue;
            }

            switch (token.Type) {
                case TokenType.LongKey: {
                    if (!options.TryGetValue(token.KeySpan, out IOption? option)) {
                        throw new UnexpectedTokenException(token);
                    }

                    if (option.IsFlag) {
                        object? latest = result.TryGetValue(option.Name, out object? value) ? value : option.Default;
                        result.AddOrUpdateOption(option.Name, option.Convert(string.Empty, latest));

                        continue;
                    }

                    wovToken = token;
                    wovOption = option;

                    continue;
                }
                case TokenType.ShortKey: {
                    if (!_aliasOptions.TryGetValue(token.KeyChar, out IOption? option)) {
                        throw new UnexpectedTokenException(token);
                    }

                    if (option.IsFlag) {
                        object? latest = result.TryGetValue(option.Name, out object? value) ? value : option.Default;
                        result.AddOrUpdateOption(option.Name, option.Convert(string.Empty, latest));

                        continue;
                    }

                    wovToken = token;
                    wovOption = option;

                    continue;
                }
                case TokenType.LongOption: {
                    if (!options.TryGetValue(token.KeySpan, out IOption? option) || option.IsFlag) {
                        throw new UnexpectedTokenException(token);
                    }

                    object? latest = result.TryGetValue(option.Name, out object? value) ? value : option.Default;
                    result.AddOrUpdateOption(option.Name, option.Convert(token.ValueSpan, latest));

                    continue;
                }
                case TokenType.ShortOption: {
                    if (!_aliasOptions.TryGetValue(token.KeyChar, out IOption? option) || option.IsFlag) {
                        throw new UnexpectedTokenException(token);
                    }

                    object? latest = result.TryGetValue(option.Name, out object? value) ? value : option.Default;
                    result.AddOrUpdateOption(option.Name, option.Convert(token.ValueSpan, latest));

                    continue;
                }
                case TokenType.Argument: {
                    if (!tokenizer.EndOfOption && result.SubCommand == null && (_commands.TryGetValue(token.Origin, out Command? command) || _aliasCommands.TryGetValue(token.Origin[0], out command))) {
                        result.SetSubCommand(command.Parse(tokenizer));
                        return result;
                    }

                    if (argumentIndex >= _arguments.Count) throw new UnexpectedTokenException(token);

                    result.AddArgument(_arguments[argumentIndex].Convert(token.Origin));
                    argumentIndex++;

                    continue;
                }
            }
        }

        return result;
    }
}
