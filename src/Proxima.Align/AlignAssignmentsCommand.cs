using System.Diagnostics;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Editor;

namespace Proxima.Align;

[VisualStudioContribution]
internal class AlignAssignmentsCommand : Command
{
    private static readonly CommandConfiguration _commandConfiguration =
        new("%Proxima.Align.AlignAssignmentsCommand.DisplayName%")
        {
            // Nessun placement diretto: il comando appare SOLO tramite ProximaAlignMenu.Children
            // L'array vuoto è necessario per registrare lo shortcut
            Placements = [],
            Icon      = new(ImageMoniker.KnownValues.AlignLeft, IconSettings.IconAndText),
            Shortcuts = [new CommandShortcutConfiguration(ModifierKey.ControlLeftAlt, Key.VK_OEM_5)],
        };

    public override CommandConfiguration CommandConfiguration => _commandConfiguration;

    private readonly AlignSettingsService _settingsService;

    public AlignAssignmentsCommand(AlignSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        try
        {
            var textView = await context.GetActiveTextViewAsync(cancellationToken);
            if (textView is null) return;

            var selection = textView.Selection;
            if (selection.IsEmpty) return;

            var selectedRange = selection.Extent;
            var selectedText  = selectedRange.CopyToString();
            var lines         = selectedText.Split('\n');
            var aligned       = AlignmentService.AlignOperators(lines, _settingsService.Current);

            if (aligned is null) return;

            var newText = string.Join('\n', aligned);
            if (newText == selectedText) return;

            await this.Extensibility.Editor().EditAsync(
                editBatch => textView.Document.AsEditable(editBatch).Replace(selectedRange, newText),
                cancellationToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Proxima.Align] Error: {ex}");
        }
    }
}