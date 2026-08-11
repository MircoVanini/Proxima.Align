using Xunit;

namespace Proxima.Align.Tests;

public sealed class AlignSettingsTests
{
    [Fact]
    public void WithAlignmentPreferences_PreservesSettingsNotExposedByEditor()
    {
        var original = new AlignSettings
        {
            AutoAlign = true,
            AlignComments = true,
            TabSize = 8,
        };

        var updated = original.WithAlignmentPreferences(
            ["=", "+="],
            spaceBeforeOperator: false,
            spaceAfterOperator: false);

        Assert.Equal(["=", "+="], updated.EnabledOperators);
        Assert.False(updated.SpaceBeforeOperator);
        Assert.False(updated.SpaceAfterOperator);
        Assert.True(updated.AutoAlign);
        Assert.True(updated.AlignComments);
        Assert.Equal(8, updated.TabSize);
    }
}
