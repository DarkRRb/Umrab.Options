using System;

using Umrab.Options.Tokenization;

namespace Umrab.Options.Exceptions;

public class MissingValueException : Exception {
    public string Name { get; }
    public string Origin { get; }

    public override string Message { get; }

    internal MissingValueException(IOption option, Token token) {
        Name = option.Name;
        Origin = token.Origin;

        Message = $"{Name} missing value. From {Origin}";
    }
}