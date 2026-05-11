using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace Proxima.Align;

/// <summary>
/// Defines the menu configuration for the Proxima.Align extension in Visual Studio.
/// </summary>
internal static class ProximaAlignMenu
{
    /// <summary>
    /// Gets the main menu configuration for Proxima.Align extension.
    /// The menu is placed in the Extensions menu and contains commands for aligning assignments
    /// and accessing alignment settings.
    /// </summary>
    /// <value>
    /// A <see cref="MenuConfiguration"/> that includes:
    /// <list type="bullet">
    /// <item><description>Placement in the Visual Studio Extensions menu</description></item>
    /// <item><description>Child command for aligning assignments (<see cref="AlignAssignmentsCommand"/>)</description></item>
    /// <item><description>Child command for opening alignment settings (<see cref="OpenAlignSettingsCommand"/>)</description></item>
    /// </list>
    /// </value>
    [VisualStudioContribution]
    public static MenuConfiguration ProximaMenu => new("%Proxima.Align.ProximaAlignMenu.DisplayName%")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Children   =
        [
            MenuChild.Command<AlignAssignmentsCommand>(),
            MenuChild.Command<OpenAlignSettingsCommand>(),
        ],
    };
}