using System.Text.RegularExpressions;

namespace Proxima.Align;

internal static class AlignmentService
{
    private static readonly string[] AllOperators =
    [
        "=>", "<<=", ">>=",
        "+=", "-=", "*=", "/=", "%=",
        "&=", "|=", "^=",
        "="
    ];

    private static readonly Regex LineCommentRegex = new(@"//.*$", RegexOptions.Compiled);

    /// <summary>
    /// Regex con 4 gruppi:
    ///   (1) parte sinistra  (2) spazi prima dell'operatore
    ///   (3) operatore       (4) resto dopo l'operatore
    /// </summary>
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

        // La colonna target (visiva) è il MAX tra tutte le righe che hanno un operatore.
        // Usa colonne visive così i tab vengono espansi correttamente.
        int operatorColumn = withOperator.Max(p =>
            p.LeftVisualWidth +
            (settings.SpaceBeforeOperator ? Math.Max(1, p.SpacesBefore) : 0));

        // Spazi dopo l'operatore: prendi il massimo trovato nel blocco
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

            // leftTrimmed = testo a sinistra dell'operatore senza spazi finali
            var leftTrimmed = p.Left!.TrimEnd();

            // Ricalcola la larghezza visiva dopo il trim (i tab iniziali rimangono)
            int leftVisual  = VisualWidth(leftTrimmed, tabSize);

            // Spazi (sempre space, mai tab) necessari per arrivare alla colonna target
            int spacesNeeded = operatorColumn - leftVisual;
            var beforePadding = new string(' ', Math.Max(settings.SpaceBeforeOperator ? 1 : 0, spacesNeeded));

            result[i] = $"{leftTrimmed}{beforePadding}{p.Operator}{paddingAfter}{p.Right}";
        }

        return result;
    }

    /// <summary>
    /// Calcola la larghezza visiva di una stringa tenendo conto dei tab.
    /// </summary>
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