using System;

using Umrab.Options.Tokenization;

namespace Umrab.Options.Exceptions;

public class UnexpectedTokenException : Exception {
    public string Origin { get; }

    public string Value { get; }

    public override string Message {get;}

    internal UnexpectedTokenException(Token token) {
        Origin = token.Origin;
        Value = token.Type switch {
            TokenType.LongKey => token.Origin,
            TokenType.ShortKey => $"-{token.KeyChar}",
            TokenType.LongOption => token.Origin,
            TokenType.ShortOption => $"-{token.KeyChar}={token.ValueSpan}",
            TokenType.Argument => token.Origin,
            _ => throw new NotImplementedException(),
        };

        Message = $"Unexpected token({Value}). From {Origin}";
    }
}