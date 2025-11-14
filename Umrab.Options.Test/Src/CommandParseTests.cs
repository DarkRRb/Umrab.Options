using System;
using System.Collections.Generic;

using Umrab.Options;
using Umrab.Options.Exceptions;

using Xunit;

namespace Umrab.Options.Test;

public class CommandParseTests {
    private static Command BuildRoot() => new Command("app")
            // Counting flag: -v / -vv / -v -vv
            .Option<int>("verbose", ['v'], isFlag: true, (span, latest) => latest + 1)
            // String option: --name foo / --name=foo / -n foo / -n=foo
            .Option<string>("name", ['n'], (span, latest) => span.ToString())
            // For short-group test: -abc=5
            .Option<bool>("a", ['a'], isFlag: true, (span, latest) => true)
            .Option<bool>("b", ['b'], isFlag: true, (span, latest) => true)
            .Option<int>("count", ['c'], (span, latest) => int.Parse(span))
            // Positional args (for end-of-options test)
            .Argument(span => span.ToString())
            .Argument(span => span.ToString())
            // Sub-command: run (alias 'r')
            .SubCommand("run", ['r'], c => c
                .Option<bool>("dry-run", ['d'], isFlag: true, (span, latest) => true)
                .Option<int>("level", ['l'], (span, latest) => int.Parse(span))
                .Argument(span => span.ToString()));

    [Fact]
    public void FlagAggregation() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["-v", "-vv"]);

        Assert.Equal(3, result.GetOption("verbose", () => 0));
    }

    [Fact]
    public void LongOptionNextArg() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["--name", "alice"]);

        Assert.Equal("alice", result.GetOption("name", () => ""));
    }

    [Fact]
    public void LongOptionWithEquals() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["--name=alice"]);

        Assert.Equal("alice", result.GetOption("name", () => ""));
    }

    [Fact]
    public void ShortOptionNextArg() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["-n", "bob"]);

        Assert.Equal("bob", result.GetOption("name", () => ""));
    }

    [Fact]
    public void ShortOptionWithEquals() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["-n=bob"]);

        Assert.Equal("bob", result.GetOption("name", () => ""));
    }

    [Fact]
    public void ShortGroupWithValueOnLast() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["-abc=5"]);

        Assert.True(result.GetOption("a", () => false));
        Assert.True(result.GetOption("b", () => false));
        Assert.Equal(5, result.GetOption("count", () => 0));
    }

    [Fact]
    public void SubCommand() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["run", "-d", "--level", "3", "target.txt"]);

        Assert.NotNull(result.SubCommand);
        ParseResult sub = result.SubCommand!;
        Assert.Equal("run", sub.Command.Name);
        Assert.True(sub.GetOption("dry-run", () => false));
        Assert.Equal(3, sub.GetOption("level", () => 0));
        Assert.Equal("target.txt", sub.GetArgument(0, () => ""));
    }

    [Fact]
    public void EndOfOptions() {
        Command root = BuildRoot();
        ParseResult result = root.Parse(["--", "--name", "-v"]);

        Assert.Equal("--name", result.GetArgument(0, () => ""));
        Assert.Equal("-v", result.GetArgument(1, () => ""));
    }

    [Fact]
    public void UnknownOption() {
        Command root = BuildRoot();
        Assert.Throws<UnexpectedTokenException>(() => root.Parse(["--unknown"]));
    }

    [Fact]
    public void MissingValueNextIsOption() {
        Command root = BuildRoot();
        Assert.Throws<MissingValueException>(() => root.Parse(["--name", "--unknown"]));
    }
}
