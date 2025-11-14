using System;

namespace Umrab.Options;

public interface IArgument {
    object Convert(ReadOnlySpan<char> value);
}

public class Argument<T>(Func<ReadOnlySpan<char>, T> converter) : IArgument {
    private readonly Func<ReadOnlySpan<char>, T> _converter = converter;

    public object Convert(ReadOnlySpan<char> value) => _converter(value)!;
}