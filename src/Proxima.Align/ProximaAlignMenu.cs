using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;

namespace Proxima.Align;

/// <summary>
/// Sottomenu "Proxima Align" dentro Extensions.
/// </summary>
internal static class ProximaAlignMenu
{
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