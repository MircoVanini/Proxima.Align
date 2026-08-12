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

    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled for alignment operations.
    /// </summary>
    public bool EnableLog { get; set; } = false;

    /// <summary>
    /// Creates a new instance of <see cref="AlignSettings"/> with updated alignment preferences while preserving other settings.
    /// </summary>
    /// <param name="enabledOperators">The list of operators to enable for alignment.</param>
    /// <param name="spaceBeforeOperator">Indicates whether a space should be inserted before the operator.</param>
    /// <param name="spaceAfterOperator">Indicates whether a space should be inserted after the operator.</param>
    /// <returns>A new <see cref="AlignSettings"/> instance with the specified alignment preferences.</returns>
    public AlignSettings WithAlignmentPreferences(List<string> enabledOperators,
                                                  bool spaceBeforeOperator,
                                                  bool spaceAfterOperator,
                                                  bool enableLog)
    {
        return new AlignSettings
        {
            EnabledOperators    = [.. enabledOperators],
            AutoAlign           = AutoAlign,
            AlignComments       = AlignComments,
            SpaceBeforeOperator = spaceBeforeOperator,
            SpaceAfterOperator  = spaceAfterOperator,
            TabSize             = TabSize,
            EnableLog           = enableLog,
        };
    }

    /// <summary>
    /// Creates a new instance of <see cref="AlignSettings"/> with an updated tab size while preserving other settings.
    /// </summary>
    /// <param name="tabSize">The new tab size to use.</param>
    /// <returns>A new <see cref="AlignSettings"/> instance with the specified tab size.</returns>
    public AlignSettings WithTabSize(int tabSize)
    {
        var copy = Copy();
        copy.TabSize = tabSize;
        return copy;
    }

    /// <summary>
    /// Creates a deep copy of the current <see cref="AlignSettings"/> instance.
    /// </summary>
    /// <returns>A new <see cref="AlignSettings"/> instance that is a deep copy of the current instance.</returns>
    public AlignSettings Copy()
    {
        return new AlignSettings
        {
            EnabledOperators    = [.. EnabledOperators],
            AutoAlign           = AutoAlign,
            AlignComments       = AlignComments,
            SpaceBeforeOperator = SpaceBeforeOperator,
            SpaceAfterOperator  = SpaceAfterOperator,
            TabSize             = TabSize,
            EnableLog           = EnableLog
        };
    }
}
