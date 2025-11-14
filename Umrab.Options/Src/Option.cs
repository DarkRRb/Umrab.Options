using System;

namespace Umrab.Options;

internal interface IOption {
    string Name { get; }
    bool IsFlag { get; }
    object? Default { get; }

    object Convert(ReadOnlySpan<char> value, object? latest);
}

public class Option<T>(string name, bool isFlag, Func<ReadOnlySpan<char>, T?, T> converter) : IOption {
    public string Name { get; } = name;
    public bool IsFlag { get; } = isFlag;
    object? IOption.Default => default(T);

    private readonly Func<ReadOnlySpan<char>, T?, T> _converter = converter;

    public object Convert(ReadOnlySpan<char> value, object? latest) => _converter(value, (T?)latest)!;
}