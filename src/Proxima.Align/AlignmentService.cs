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

        var parsed = lines
            .Select(line => ParseLine(line, operatorRegex, settings.AlignComments))
            .ToArray();

        var withOperator = parsed.Where(p => p.Operator is not null).ToList();

        if (withOperator.Count < 2)
            return null;

        // La colonna target dell'operatore è il MAX di (lunghezza sinistra + spazi prima)
        // su tutte le righe del blocco.
        //
        // Questo garantisce:
        //  - Se le righe sono già allineate alla stessa colonna → quella colonna viene mantenuta.
        //  - Se una riga ha la parte sinistra più lunga → le altre vengono portate alla sua colonna.
        //
        // Esempio:
        //   "var a  +="  → left=5, spaces=2  → col=7
        //   "var b1 ="   → left=6, spaces=1  → col=7   ← già alla stessa colonna, si mantiene
        //   "var b2 ="   → left=6, spaces=1  → col=7
        //
        // Risultato: operatorColumn=7, tutte rimangono invariate.
        int operatorColumn = withOperator.Max(p =>
            p.Left!.TrimEnd().Length +
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

            var leftTrimmed = p.Left!.TrimEnd();

            // Spazi necessari per portare l'operatore alla colonna target
            int spacesNeeded = operatorColumn - leftTrimmed.Length;
            var beforePadding = new string(' ', Math.Max(settings.SpaceBeforeOperator ? 1 : 0, spacesNeeded));

            result[i] = $"{leftTrimmed}{beforePadding}{p.Operator}{paddingAfter}{p.Right}";
        }

        return result;
    }

    private static ParsedLine ParseLine(string line, Regex operatorRegex, bool alignComments)
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
            return new ParsedLine(line, null, 0, null, 0, null);

        // Gruppo 4 = spazi dopo operatore + valore destro — separati
        var rawRight = match.Groups[4].Value;
        var spacesAfter = rawRight.Length - rawRight.TrimStart().Length;
        var rightValue = rawRight.TrimStart() + (trailingComment ?? "");

        return new ParsedLine(
            Original: line,
            Left: match.Groups[1].Value,
            SpacesBefore: match.Groups[2].Value.Length,
            Operator: match.Groups[3].Value,
            SpacesAfter: spacesAfter,
            Right: rightValue);
    }

    private record ParsedLine
    (
        string Original,
        string? Left,
        int SpacesBefore,
        string? Operator,
        int SpacesAfter,
        string? Right
    );
}