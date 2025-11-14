using System;

namespace Umrab.Options.Tokenization;

internal readonly struct Token(TokenType type, string origin, int keyStart, int keyLength, int valueStart, int valueLength) {
    public TokenType Type { get; } = type;

    public string Origin { get; } = origin;

    public int KeyStart { get; } = keyStart;

    public int KeyLength { get; } = keyLength;

    public int ValueStart { get; } = valueStart;

    public int ValueLength { get; } = valueLength;

    public ReadOnlySpan<char> KeySpan => Origin.AsSpan(KeyStart, KeyLength);

    public ReadOnlySpan<char> ValueSpan => Origin.AsSpan(ValueStart, ValueLength);

    public char KeyChar => Origin[KeyStart];

    public static Token LongKey(string origin) {
        return new Token(TokenType.LongKey, origin, 2, origin.Length - 2, 0, 0);
    }

    public static Token ShortKey(string origin, int keyStart) {
        return new Token(TokenType.ShortKey, origin, keyStart, 0, 0, 0);
    }

    public static Token LongOption(string origin, int equalsIndex) {
        return new Token(TokenType.LongOption, origin, 2, equalsIndex - 2, equalsIndex + 1, origin.Length - equalsIndex - 1);
    }

    public static Token ShortOption(string origin, int keyStart) {
        return new Token(TokenType.ShortOption, origin, keyStart, 0, keyStart + 2, origin.Length - keyStart - 2);
    }

    public static Token Argument(string origin) {
        return new Token(TokenType.Argument, origin, 0, 0, 0, 0);
    }
}