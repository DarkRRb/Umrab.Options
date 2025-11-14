using System;

using Umrab.Options.Tokenization;

using Xunit;

namespace Umrab.Options.Test.Tokenization;

public class TokenizerTests {
    [Fact]
    public void Empty() {
        string[] args = [];
        Tokenizer tokenizer = new(args);

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void LongKey() {
        string[] args = ["--key"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.LongKey, token.Type);
            Assert.Equal("--key", token.Origin);
            Assert.Equal("key", token.KeySpan);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void LongOption() {
        string[] args = ["--key=value"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.LongOption, token.Type);
            Assert.Equal("--key=value", token.Origin);
            Assert.Equal("key", token.KeySpan);
            Assert.Equal("value", token.ValueSpan);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void ShortKey() {
        string[] args = ["-k"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortKey, token.Type);
            Assert.Equal("-k", token.Origin);
            Assert.Equal('k', token.KeyChar);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void ShortKeys() {
        string[] args = ["-key"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortKey, token.Type);
            Assert.Equal("-key", token.Origin);
            Assert.Equal('k', token.KeyChar);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortKey, token.Type);
            Assert.Equal("-key", token.Origin);
            Assert.Equal('e', token.KeyChar);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortKey, token.Type);
            Assert.Equal("-key", token.Origin);
            Assert.Equal('y', token.KeyChar);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void ShortOption() {
        string[] args = ["-k=value"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortOption, token.Type);
            Assert.Equal("-k=value", token.Origin);
            Assert.Equal('k', token.KeyChar);
            Assert.Equal("value", token.ValueSpan);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void ShortKeysAndShortOption() {
        string[] args = ["-key=value"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortKey, token.Type);
            Assert.Equal("-key=value", token.Origin);
            Assert.Equal('k', token.KeyChar);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortKey, token.Type);
            Assert.Equal("-key=value", token.Origin);
            Assert.Equal('e', token.KeyChar);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.ShortOption, token.Type);
            Assert.Equal("-key=value", token.Origin);
            Assert.Equal('y', token.KeyChar);
            Assert.Equal("value", token.ValueSpan);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void Argument() {
        string[] args = ["value", "-"];
        Tokenizer tokenizer = new(args);

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("value", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("-", token.Origin);
        }

        Assert.False(tokenizer.TryNext(out _));
    }

    [Fact]
    public void EndOfOption() {
        string[] args = ["-", "--", "--key", "--key=value", "-k", "-k=value", "-key", "-key=value"];
        Tokenizer tokenizer = new(args.AsSpan());

        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.False(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("-", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.True(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("--key", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.True(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("--key=value", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.True(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("-k", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.True(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("-k=value", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.True(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("-key", token.Origin);
        }
        {
            Assert.True(tokenizer.TryNext(out Token token));
            Assert.True(tokenizer.EndOfOption);
            Assert.Equal(TokenType.Argument, token.Type);
            Assert.Equal("-key=value", token.Origin);
        }

        Assert.False(tokenizer.TryNext(out _));
    }
}
