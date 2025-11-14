using System;

namespace Umrab.Options.Tokenization;

internal ref struct Tokenizer(ReadOnlySpan<string> args) {
    private readonly ReadOnlySpan<string> _args = args;

    private int _index = 0;
    private int _charIndex = 0;

    public bool EndOfOption { get; private set; } = false;
    public bool TryNext(out Token token) {
        if (_index >= _args.Length) {
            token = default;
            return false;
        }

        string arg = _args[_index];

        if (_charIndex != 0) {
            if (_charIndex + 1 < arg.Length && arg[_charIndex + 1] == '=') {
                token = Token.ShortOption(arg, _charIndex);
                _index++;
                _charIndex = 0;
                return true;
            }

            token = Token.ShortKey(arg, _charIndex);
            if (_charIndex + 1 < arg.Length) {
                _charIndex++;
            } else {
                _index++;
                _charIndex = 0;
            }
            return true;
        }

        if (EndOfOption) {
            token = Token.Argument(arg);
            _index++;
            return true;
        }

        if (arg.StartsWith("--", StringComparison.Ordinal)) {
            if (arg.Length == 2) {
                EndOfOption = true;
                _index++;
                return TryNext(out token);
            }

            int equalsIndex = arg.IndexOf('=', 2);
            if (equalsIndex != -1) {
                token = Token.LongOption(arg, equalsIndex);
                _index++;
                return true;
            }

            token = Token.LongKey(arg);
            _index++;
            return true;
        }

        if (arg.StartsWith('-')) {
            if (arg.Length == 1) {
                token = Token.Argument(arg);
                _index++;
                return true;
            }

            _charIndex = 1;
            return TryNext(out token);
        }

        token = Token.Argument(arg);
        _index++;
        return true;
    }
}