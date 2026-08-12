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
    /// Aligns operators across multiple lines of code by adding appropriate spacing.
    /// </summary>
    /// <param name="lines">The lines of code to align.</param>
    /// <param name="settings">Settings that control alignment behavior, including which operators to align and spacing preferences.</param>
    /// <param name="precedingText">Document text preceding the first line, used to establish multiline lexical state.</param>
    /// <returns>
    /// An array of aligned lines if alignment is possible (at least 2 lines with operators found),
    /// or <c>null</c> if alignment cannot be performed (empty input or fewer than 2 lines with operators).
    /// </returns>
    public static string[]? AlignOperators(string[] lines,
                                           AlignSettings settings,
                                           string? precedingText = null)
    {
        if (lines.Length == 0)
            return null;

        var enabledOperators = AllOperators.Where(settings.EnabledOperators.Contains)
                                           .ToArray();

        int tabSize = settings.TabSize > 0 ? settings.TabSize : 4;
        var lexerState = new LexerState();

        if (!string.IsNullOrEmpty(precedingText))
        {
            foreach (var precedingLine in precedingText.Split('\n'))
            {
                ParseLine(precedingLine.TrimEnd('\r'),
                          [],
                          alignComments: false,
                          tabSize,
                          lexerState);
            }
        }

        var parsed = lines
            .Select(line => ParseLine(line, enabledOperators, settings.AlignComments, tabSize, lexerState))
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
    /// <param name="enabledOperators">The enabled operators, ordered from longest to shortest.</param>
    /// <param name="alignComments">If <c>false</c>, operators within line comments are ignored.</param>
    /// <param name="tabSize">The tab size for calculating visual width.</param>
    /// <param name="lexerState">Lexical state carried across lines for multiline comments and strings.</param>
    /// <returns>A <see cref="ParsedLine"/> record containing the parsed components and metadata.</returns>
    private static ParsedLine ParseLine(string line,
                                        string[] enabledOperators,
                                        bool alignComments,
                                        int tabSize,
                                        LexerState lexerState)
    {
        int operatorIndex = -1;
        string? matchedOperator = null;
        bool inLineComment = false;

        for (int i = 0; i < line.Length;)
        {
            if (lexerState.RawStringQuoteCount > 0)
            {
                int quoteCount = CountRun(line, i, '"');
                if (quoteCount >= lexerState.RawStringQuoteCount)
                {
                    lexerState.RawStringQuoteCount = 0;
                    i += quoteCount;
                }
                else
                {
                    i += Math.Max(1, quoteCount);
                }
                continue;
            }

            if (lexerState.InVerbatimString)
            {
                if (line[i] == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    i += 2;
                }
                else if (line[i] == '"')
                {
                    lexerState.InVerbatimString = false;
                    i++;
                }
                else
                {
                    i++;
                }
                continue;
            }

            if (lexerState.InBlockComment)
            {
                if (i + 1 < line.Length && line[i] == '*' && line[i + 1] == '/')
                {
                    lexerState.InBlockComment = false;
                    i += 2;
                    continue;
                }

                if (alignComments && matchedOperator is null &&
                    TryMatchOperator(line, i, enabledOperators, out var commentOperator))
                {
                    operatorIndex = i;
                    matchedOperator = commentOperator;
                    i += commentOperator.Length;
                    continue;
                }

                i++;
                continue;
            }

            if (inLineComment)
            {
                if (alignComments && matchedOperator is null &&
                    TryMatchOperator(line, i, enabledOperators, out var commentOperator))
                {
                    operatorIndex = i;
                    matchedOperator = commentOperator;
                    i += commentOperator.Length;
                }
                else
                {
                    i++;
                }
                continue;
            }

            if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '/')
            {
                inLineComment = true;
                i += 2;
                continue;
            }

            if (i + 1 < line.Length && line[i] == '/' && line[i + 1] == '*')
            {
                lexerState.InBlockComment = true;
                i += 2;
                continue;
            }

            if (line[i] == '"')
            {
                int quoteCount = CountRun(line, i, '"');
                if (quoteCount >= 3)
                {
                    lexerState.RawStringQuoteCount = quoteCount;
                    i += quoteCount;
                }
                else
                {
                    bool isVerbatim = i > 0 && line[i - 1] == '@';
                    i = SkipQuotedLiteral(line, i + 1, '"', isVerbatim, out bool terminated);
                    lexerState.InVerbatimString = isVerbatim && !terminated;
                }
                continue;
            }

            if (line[i] == '\'')
            {
                i = SkipQuotedLiteral(line, i + 1, '\'', false, out _);
                continue;
            }

            if (matchedOperator is null &&
                TryMatchOperator(line, i, enabledOperators, out var codeOperator))
            {
                operatorIndex = i;
                matchedOperator = codeOperator;
                i += codeOperator.Length;
                continue;
            }

            i++;
        }

        if (matchedOperator is null)
            return new ParsedLine(line, null, 0, 0, null, 0, null);

        var leftRaw = line[..operatorIndex];
        var spacesBefore = leftRaw.Length - leftRaw.TrimEnd().Length;
        var leftVisual = VisualWidth(leftRaw.TrimEnd(), tabSize);

        var rawRight = line[(operatorIndex + matchedOperator.Length)..];
        var spacesAfter = rawRight.Length - rawRight.TrimStart().Length;
        var rightValue = rawRight.TrimStart();

        return new ParsedLine(
            Original:        line,
            Left:            leftRaw,
            SpacesBefore:    spacesBefore,
            LeftVisualWidth: leftVisual,
            Operator:        matchedOperator,
            SpacesAfter:     spacesAfter,
            Right:           rightValue);
    }

    private static bool TryMatchOperator(string line,
                                         int index,
                                         string[] enabledOperators,
                                         out string matchedOperator)
    {
        foreach (var candidate in enabledOperators)
        {
            if (index + candidate.Length > line.Length ||
                !line.AsSpan(index, candidate.Length).SequenceEqual(candidate))
            {
                continue;
            }

            if (candidate == "=" &&
                ((index > 0 && IsAdjacentOperatorCharacter(line[index - 1])) ||
                 (index + 1 < line.Length && line[index + 1] is '=' or '>')))
            {
                continue;
            }

            matchedOperator = candidate;
            return true;
        }

        matchedOperator = string.Empty;
        return false;
    }

    /// <summary>
    /// Determines if a character is adjacent to an operator, which affects whether a simple assignment operator '=' should be matched.
    /// </summary>
    /// <param name="value">The character to evaluate.</param>
    /// <returns><c>true</c> if the character is adjacent to an operator; otherwise, <c>false</c>.</returns>
    private static bool IsAdjacentOperatorCharacter(char value)
        => value is '=' or '!' or '<' or '>' or '+' or '-' or '*' or '/' or '%' or '&' or '|' or '^';

    /// <summary>
    /// Counts the number of consecutive occurrences of a specified character in a string starting from a given index.
    /// </summary>
    /// <param name="text">The string to evaluate.</param>
    /// <param name="start">The starting index within the string.</param>
    /// <param name="value">The character to count.</param>
    /// <returns>The number of consecutive occurrences of the specified character.</returns>
    private static int CountRun(string text, int start, char value)
    {
        int end = start;
        while (end < text.Length && text[end] == value)
            end++;
        return end - start;
    }

    /// <summary>
    /// Skips over a quoted literal in a line of code, handling escape sequences and verbatim strings.
    /// </summary>
    /// <param name="line">The line of code to evaluate.</param>
    /// <param name="index">The starting index within the line.</param>
    /// <param name="delimiter">The character that delimits the quoted literal.</param>
    /// <param name="verbatim">Indicates whether the string is a verbatim string.</param>
    /// <param name="terminated">Outputs whether the quoted literal was properly terminated.</param>
    /// <returns>The index immediately after the quoted literal.</returns>
    private static int SkipQuotedLiteral(string line,
                                         int index,
                                         char delimiter,
                                         bool verbatim,
                                         out bool terminated)
    {
        while (index < line.Length)
        {
            if (verbatim && line[index] == delimiter &&
                index + 1 < line.Length && line[index + 1] == delimiter)
            {
                index += 2;
                continue;
            }

            if (!verbatim && line[index] == '\\')
            {
                index += Math.Min(2, line.Length - index);
                continue;
            }

            if (line[index] == delimiter)
            {
                terminated = true;
                return index + 1;
            }

            index++;
        }

        terminated = false;
        return index;
    }

    /// <summary>
    /// Represents the lexical state of the parser, tracking whether it is currently inside a block comment, verbatim string, or raw string literal.
    /// </summary>
    private sealed class LexerState
    {
        public bool InBlockComment { get; set; }
        public bool InVerbatimString { get; set; }
        public int RawStringQuoteCount { get; set; }
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