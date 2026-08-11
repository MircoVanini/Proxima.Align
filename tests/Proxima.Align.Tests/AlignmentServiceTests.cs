using Xunit;

namespace Proxima.Align.Tests;

public sealed class AlignmentServiceTests
{
    [Fact]
    public void AlignOperators_IgnoresOperatorsInsideStringLiterals()
    {
        string[] lines =
        [
            """Console.WriteLine("a = 1");""",
            """Console.WriteLine("longer => value");""",
        ];

        var result = AlignmentService.AlignOperators(lines, new AlignSettings());

        Assert.Null(result);
    }

    [Fact]
    public void AlignOperators_AlignsAssignmentsWithoutChangingStringContents()
    {
        string[] lines =
        [
            """var a = "x = y";""",
            """var longerName = "x => y";""",
        ];

        var result = AlignmentService.AlignOperators(lines, new AlignSettings());

        Assert.NotNull(result);
        Assert.Equal(
        [
            """var a          = "x = y";""",
            """var longerName = "x => y";""",
        ], result);
    }

    [Fact]
    public void AlignOperators_IgnoresLineAndMultilineComments()
    {
        string[] lines =
        [
            "// comment = value",
            "/* block = value",
            "   continued => value */",
            "var a = 1;",
            "var longer = 2;",
        ];

        var result = AlignmentService.AlignOperators(lines, new AlignSettings());

        Assert.NotNull(result);
        Assert.Equal(
        [
            "// comment = value",
            "/* block = value",
            "   continued => value */",
            "var a      = 1;",
            "var longer = 2;",
        ], result);
    }

    [Fact]
    public void AlignOperators_IgnoresOperatorsInsideVerbatimAndRawStrings()
    {
        string[] lines =
        [
            """Log(@"first = value");""",
            """"""Log("""second => value""");"""""",
        ];

        var result = AlignmentService.AlignOperators(lines, new AlignSettings());

        Assert.Null(result);
    }

    [Fact]
    public void AlignOperators_TracksMultilineVerbatimAndRawStrings()
    {
        string[] lines =
        [
            "Consume(@\"first = value",
            "continued => value\");",
            "Consume(\"\"\"first = value",
            "continued => value\"\"\");",
            "var a = 1;",
            "var longer = 2;",
        ];

        var result = AlignmentService.AlignOperators(lines, new AlignSettings());

        Assert.NotNull(result);
        Assert.Equal(
        [
            "Consume(@\"first = value",
            "continued => value\");",
            "Consume(\"\"\"first = value",
            "continued => value\"\"\");",
            "var a      = 1;",
            "var longer = 2;",
        ], result);
    }

    [Fact]
    public void AlignOperators_UsesPrecedingContextForPartialMultilineSelections()
    {
        string[] lines =
        [
            "comment = value */",
            "var a = 1;",
            "var longer = 2;",
        ];

        var result = AlignmentService.AlignOperators(
            lines,
            new AlignSettings(),
            "/* block comment opened above\n");

        Assert.NotNull(result);
        Assert.Equal(
        [
            "comment = value */",
            "var a      = 1;",
            "var longer = 2;",
        ], result);
    }

    [Fact]
    public void AlignOperators_RecognizesAssignmentBeforeUnaryExpression()
    {
        string[] lines =
        [
            "var a =-1;",
            "var longer =-2;",
        ];

        var result = AlignmentService.AlignOperators(lines, new AlignSettings());

        Assert.NotNull(result);
        Assert.Equal(
        [
            "var a      = -1;",
            "var longer = -2;",
        ], result);
    }

    [Fact]
    public void AlignOperators_DoesNotTreatDisabledLambdaAsAssignment()
    {
        string[] lines =
        [
            "items.Select(item => item.Value);",
            "values.Select(value => value.Name);",
        ];
        var settings = new AlignSettings { EnabledOperators = ["="] };

        var result = AlignmentService.AlignOperators(lines, settings);

        Assert.Null(result);
    }

    [Fact]
    public void AlignOperators_CanAlignOperatorsInCommentsWhenEnabled()
    {
        string[] lines =
        [
            "// a = first",
            "// longer = second",
        ];
        var settings = new AlignSettings { AlignComments = true };

        var result = AlignmentService.AlignOperators(lines, settings);

        Assert.NotNull(result);
        Assert.Equal(
        [
            "// a      = first",
            "// longer = second",
        ], result);
    }
}
