using System.Text.RegularExpressions;

namespace Proxima.Align;

/// <summary>
/// Provides services for aligning assignment and other operators across multiple lines of code.
/// </summary>
internal static class AlignmentService
{
    /// <summary>
    /// All supported operators for alignment, ordered by precedence for matching.
    /// Compound assignment operators are listed before simple assignment to ensure correct matching.
    /// </summary>
    private static readonly string[] AllOperators =
    [
        "=>", "<<=", ">>=",
        "+=", "-=", "*=", "/=", "%=",
        "&=", "|=", "^=",
        "="
    ];

    /// <summary>
    /// Regular expression for matching line comments (// style) to the end of a line.
    /// </summary>
    private static readonly Regex LineCommentRegex = new(@"//.*$", RegexOptions.Compiled);

    /// <summary>
    /// Builds a regular expression to match operators on a line.
    /// Regex with 4 groups:
    ///   (1) left part  (2) spaces before the operator
    ///   (3) operator   (4) remainder after the operator
    /// </summary>
    /// <param name="enabledOperators">The collection of operators that should be matched.</param>
    /// <returns>A compiled regular expression that matches the enabled operators, or a never-matching regex if no operators are enabled.</returns>
    private static Regex BuildOperatorRegex(IEnumerable<string> enabledOperators)
    {
        var ordered = AllOperators.Where(enabledOperators.Contains).ToList();
        if (ordered.Count == 0)
            return new Regex("(?!)", RegexOptions.Compiled);

        var parts = ordered.Select(op =>
            op == "=" ? @"(?<![=!<>+\-*/%&|^])=(?!=)" : Regex.Escape(op));

        var pattern = $@"^(.*?)(\s*)({string.Join("|", parts)})(\s*.*)$";
        return new Regex(pattern, RegexOptions.Compiled);
    }

    /// <summary>
    /// Aligns operators across multiple lines of code by adding appropriate spacing.
    /// </summary>
    /// <param name="lines">The lines of code to align.</param>
    /// <param name="settings">Settings that control alignment behavior, including which operators to align and spacing preferences.</param>
    /// <returns>
    /// An array of aligned lines if alignment is possible (at least 2 lines with operators found),
    /// or <c>null</c> if alignment cannot be performed (empty input or fewer than 2 lines with operators).
    /// </returns>
    public static string[]? AlignOperators(string[] lines, AlignSettings settings)
    {
        if (lines.Length == 0)
            return null;

        var operatorRegex = BuildOperatorRegex(settings.EnabledOperators);
        int tabSize       = settings.TabSize > 0 ? settings.TabSize : 4;

        var parsed = lines
            .Select(line => ParseLine(line, operatorRegex, settings.AlignComments, tabSize))
            .ToArray();

        var withOperator = parsed.Where(p => p.Operator is not null).ToList();

        if (withOperator.Count < 2)
            return null;

        // The target (visual) column is the MAX among all lines that have an operator.
        // Use visual columns so tabs are expanded correctly.
        int operatorColumn = withOperator.Max(p =>
            p.LeftVisualWidth +
            (settings.SpaceBeforeOperator ? Math.Max(1, p.SpacesBefore) : 0));

        // Spaces after the operator: take the maximum found in the block
        int maxSpacesAfter = settings.SpaceAfterOperator
            ? Math.Max(1, withOperator.Max(p => p.SpacesAfter))
            : 0;

        var paddingAfter = new string(' ', maxSpacesAfter);

        var result = new string[lines.Length];
        for (int i = 0; i < parsed.Length; i++)
        {
            var p = parsed[i];
            if (p.Operator is null)
            {
                result[i] = lines[i];
                continue;
            }

            // leftTrimmed = text to the left of the operator without trailing spaces
            var leftTrimmed = p.Left!.TrimEnd();

            // Recalculate the visual width after trim (initial tabs remain)
            int leftVisual  = VisualWidth(leftTrimmed, tabSize);

            // Spaces (always space, never tab) needed to reach the target column
            int spacesNeeded = operatorColumn - leftVisual;
            var beforePadding = new string(' ', Math.Max(settings.SpaceBeforeOperator ? 1 : 0, spacesNeeded));

            result[i] = $"{leftTrimmed}{beforePadding}{p.Operator}{paddingAfter}{p.Right}";
        }

        return result;
    }

    /// <summary>
    /// Calcola la larghezza visiva di una stringa tenendo conto dei tab.
    /// Each tab character advances to the next tab stop position based on the specified tab size.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <param name="tabSize">The number of spaces per tab stop.</param>
    /// <returns>The visual column position after rendering the text.</returns>
    private static int VisualWidth(string text, int tabSize)
    {
        int col = 0;
        foreach (char ch in text)
        {
            if (ch == '\t')
                col += tabSize - (col % tabSize);
            else
                col++;
        }
        return col;
    }

    /// <summary>
    /// Parses a single line of code to extract operator and spacing information.
    /// Handles line comments by optionally excluding them from operator matching.
    /// </summary>
    /// <param name="line">The line of code to parse.</param>
    /// <param name="operatorRegex">The regular expression to use for matching operators.</param>
    /// <param name="alignComments">If <c>false</c>, operators within line comments are ignored.</param>
    /// <param name="tabSize">The tab size for calculating visual width.</param>
    /// <returns>A <see cref="ParsedLine"/> record containing the parsed components and metadata.</returns>
    private static ParsedLine ParseLine(string line, Regex operatorRegex, bool alignComments, int tabSize)
    {
        string matchTarget = line;
        string? trailingComment = null;

        if (!alignComments)
        {
            var commentMatch = LineCommentRegex.Match(line);
            if (commentMatch.Success)
            {
                matchTarget = line[..commentMatch.Index];
                trailingComment = line[commentMatch.Index..];
            }
        }

        var match = operatorRegex.Match(matchTarget);
        if (!match.Success)
            return new ParsedLine(line, null, 0, 0, null, 0, null);

        var leftRaw     = match.Groups[1].Value;   // include trailing spaces
        var spacesBefore = match.Groups[2].Value.Length;
        var leftVisual  = VisualWidth(leftRaw.TrimEnd(), tabSize);

        var rawRight    = match.Groups[4].Value;
        var spacesAfter = rawRight.Length - rawRight.TrimStart().Length;
        var rightValue  = rawRight.TrimStart() + (trailingComment ?? "");

        return new ParsedLine(
            Original:        line,
            Left:            leftRaw,
            SpacesBefore:    spacesBefore,
            LeftVisualWidth: leftVisual,
            Operator:        match.Groups[3].Value,
            SpacesAfter:     spacesAfter,
            Right:           rightValue);
    }

    /// <summary>
    /// Represents a parsed line of code containing information about an operator and surrounding content.
    /// </summary>
    /// <param name="Original">The original unmodified line.</param>
    /// <param name="Left">The content to the left of the operator, including any trailing spaces. <c>null</c> if no operator found.</param>
    /// <param name="SpacesBefore">The number of spaces immediately before the operator.</param>
    /// <param name="LeftVisualWidth">The visual width of the left content (excluding trailing spaces) accounting for tabs.</param>
    /// <param name="Operator">The matched operator. <c>null</c> if no operator found.</param>
    /// <param name="SpacesAfter">The number of spaces immediately after the operator.</param>
    /// <param name="Right">The content to the right of the operator (after leading spaces are trimmed), including any trailing comment. <c>null</c> if no operator found.</param>
    private record ParsedLine
    (
        string  Original,
        string? Left,
        int     SpacesBefore,
        int     LeftVisualWidth,
        string? Operator,
        int     SpacesAfter,
        string? Right
    );
}