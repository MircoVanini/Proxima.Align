namespace Proxima.Align;

/// <summary>
/// Configuration settings for the Proxima.Align code alignment functionality.
/// </summary>
internal sealed class AlignSettings
{
    /// <summary>
    /// Gets or sets the list of operators that will be used for alignment.
    /// </summary>
    public List<string> EnabledOperators { get; set; } =
    [
        "=", "+=", "-=", "*=", "/=", "%=",
        "&=", "|=", "^=", "<<=", ">>=", "=>"
    ];

    /// <summary>
    /// Gets or sets a value indicating whether logging is enabled for alignment operations.
    /// Default is <c>false</c>.
    /// </summary>
    public bool EnableLog { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether operators within comments should be aligned.
    /// Default is <c>false</c>.
    /// </summary>
    public bool AlignComments { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether a space should be inserted before the operator.
    /// Default is <c>true</c>.
    /// </summary>
    public bool SpaceBeforeOperator { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a space should be inserted after the operator.
    /// Default is <c>true</c>.
    /// </summary>
    public bool SpaceAfterOperator { get; set; } = true;

    /// <summary>
    /// Gets or sets the tab size used for calculating indentation during alignment.
    /// Default is 4.
    /// </summary>
    public int TabSize { get; set; } = 4;

    /// <summary>
    /// Returns a copy of the current settings with a different <see cref="TabSize"/>.
    /// <see cref="EnabledOperators"/> is deep-copied to prevent shared-list mutation.
    /// </summary>
    /// <param name="tabSize">The tab size to apply in the copy.</param>
    /// <returns>A new <see cref="AlignSettings"/> with the same values and the specified tab size.</returns>
    public AlignSettings WithTabSize(int tabSize) => new()
    {
        EnabledOperators    = new List<string>(EnabledOperators),   // ✅ deep copy
        SpaceBeforeOperator = SpaceBeforeOperator,
        SpaceAfterOperator  = SpaceAfterOperator,
        AlignComments       = AlignComments,
        EnableLog           = EnableLog,
        TabSize             = tabSize
    };
}
