namespace Proxima.Align;

/// <summary>
/// Configuration settings for the Proxima.Align code alignment functionality.
/// </summary>
internal sealed class AlignSettings
{
    /// <summary>
    /// Gets or sets the list of operators that will be used for alignment.
    /// </summary>
    /// <value>
    /// A list of operator strings. Default includes assignment operators (=, +=, -=, *=, /=, %=),
    /// bitwise assignment operators (&=, |=, ^=, <<=, >>=), and lambda operator (=>).
    /// </value>
    public List<string> EnabledOperators { get; set; } =
    [
        "=", "+=", "-=", "*=", "/=", "%=",
        "&=", "|=", "^=", "<<=", ">>=", "=>"
    ];

    /// <summary>
    /// Gets or sets a value indicating whether code should be automatically aligned as you type.
    /// </summary>
    /// <value>
    /// <c>true</c> if automatic alignment is enabled; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    public bool AutoAlign { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether comments should be aligned along with code.
    /// </summary>
    /// <value>
    /// <c>true</c> if comment alignment is enabled; otherwise, <c>false</c>. Default is <c>false</c>.
    /// </value>
    public bool AlignComments { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether a space should be inserted before the operator during alignment.
    /// </summary>
    /// <value>
    /// <c>true</c> if space before operator is enabled; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool SpaceBeforeOperator { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a space should be inserted after the operator during alignment.
    /// </summary>
    /// <value>
    /// <c>true</c> if space after operator is enabled; otherwise, <c>false</c>. Default is <c>true</c>.
    /// </value>
    public bool SpaceAfterOperator { get; set; } = true;

    /// <summary>
    /// Gets or sets the tab size used for calculating indentation during alignment.
    /// </summary>
    /// <value>
    /// The number of spaces per tab. Default is 4.
    /// </value>
    public int  TabSize { get; set; } = 4;

    public AlignSettings WithAlignmentPreferences(
        List<string> enabledOperators,
        bool spaceBeforeOperator,
        bool spaceAfterOperator)
    {
        return new AlignSettings
        {
            EnabledOperators = [.. enabledOperators],
            AutoAlign = AutoAlign,
            AlignComments = AlignComments,
            SpaceBeforeOperator = spaceBeforeOperator,
            SpaceAfterOperator = spaceAfterOperator,
            TabSize = TabSize,
        };
    }

    public AlignSettings WithTabSize(int tabSize)
    {
        var copy = Copy();
        copy.TabSize = tabSize;
        return copy;
    }

    public AlignSettings Copy()
    {
        return new AlignSettings
        {
            EnabledOperators = [.. EnabledOperators],
            AutoAlign = AutoAlign,
            AlignComments = AlignComments,
            SpaceBeforeOperator = SpaceBeforeOperator,
            SpaceAfterOperator = SpaceAfterOperator,
            TabSize = TabSize,
        };
    }
}
