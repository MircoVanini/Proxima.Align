namespace Proxima.Align;

internal sealed class AlignSettings
{
    public List<string> EnabledOperators { get; set; } =
    [
        "=", "+=", "-=", "*=", "/=", "%=",
        "&=", "|=", "^=", "<<=", ">>=", "=>"
    ];

    public bool AutoAlign { get; set; } = false;
    public bool AlignComments { get; set; } = false;
    public bool SpaceBeforeOperator { get; set; } = true;
    public bool SpaceAfterOperator { get; set; } = true;
    public int  TabSize { get; set; } = 4;
}
