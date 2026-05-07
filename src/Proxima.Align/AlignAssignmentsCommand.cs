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

            var document  = textView.Document;
            var fullText  = document.Text.CopyToString();
            var selExtent = selection.Extent;

            // Offset base del documento (può essere non-zero in VS)
            int docBase = document.Text.Start.Offset;

            // Indici nel buffer fullText (0-based rispetto a fullText[0])
            int selStart = selExtent.Start.Offset - docBase;
            int selEnd   = selExtent.End.Offset   - docBase;

            if (selStart < 0 || selEnd > fullText.Length || selStart >= selEnd) return;

            // Rileva il tipo di line ending usato nel documento
            var lineEnding = fullText.Contains("\r\n") ? "\r\n" : "\n";

            // Ricerca all'indietro l'inizio della riga che contiene selStart
            int startIdx = selStart;
            while (startIdx > 0 && fullText[startIdx - 1] != '\n')
                startIdx--;

            // Se selEnd cade esattamente all'inizio di una nuova riga (cursore in col 0)
            // o su un carattere di line break, torna indietro fino all'ultimo carattere
            // di contenuto dell'ultima riga selezionata.
            int endAdj = selEnd;
            while (endAdj > startIdx)
            {
                char cur  = endAdj < fullText.Length ? fullText[endAdj]     : '\0';
                char prev = endAdj > 0               ? fullText[endAdj - 1] : '\0';

                if (cur == '\r' || cur == '\n')
                    endAdj--;          // siamo su un line break → vai indietro
                else if (prev == '\n' || prev == '\r')
                    endAdj--;          // siamo all'inizio di una riga → vai indietro
                else
                    break;
            }

            // Ricerca in avanti la fine della riga che contiene endAdj (escluso il line break)
            int endIdx = endAdj;
            while (endIdx < fullText.Length && fullText[endIdx] != '\r' && fullText[endIdx] != '\n')
                endIdx++;

            if (endIdx <= startIdx) return;

            // Estrae le righe pulite (senza \r residui da CRLF)
            var blockText = fullText.Substring(startIdx, endIdx - startIdx);
            var lines     = blockText.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

            Debug.WriteLine($"[Proxima.Align] docBase={docBase} selStart={selStart} selEnd={selEnd}");
            Debug.WriteLine($"[Proxima.Align] startIdx={startIdx} endIdx={endIdx} lines={lines.Length}");
            for (int i = 0; i < lines.Length; i++)
                Debug.WriteLine($"[Proxima.Align]   line[{i}]: '{lines[i]}'");

            // Legge il tab size dal documento (default 4)
            int tabSize = 4;
            try
            {
                var tabOpt = await document.GetEditorOptionValueAsync(
                    new TextDocumentOption<int>("tab_size"), cancellationToken);
                tabSize = tabOpt.ValueOrDefault(4);
            }
            catch { /* usa il default */ }

            var settings = _settingsService.Current;
            settings.TabSize = tabSize;

            var aligned   = AlignmentService.AlignOperators(lines, settings);

            if (aligned is null)
            {
                Debug.WriteLine($"[Proxima.Align] AlignOperators returned null (< 2 lines with operators?)");
                return;
            }

            Debug.WriteLine($"[Proxima.Align] aligned:");
            for (int i = 0; i < aligned.Length; i++)
                Debug.WriteLine($"[Proxima.Align]   aligned[{i}]: '{aligned[i]}'");

            var newText      = string.Join(lineEnding, aligned);
            var originalText = string.Join(lineEnding, lines);
            if (newText == originalText) return;

            // Costruisce il TextRange usando posizioni assolute (docBase + indice)
            var rangeStart = document.Text.Start + startIdx;
            var rangeEnd   = document.Text.Start + endIdx;
            var blockRange = new TextRange(rangeStart, rangeEnd);

            await this.Extensibility.Editor().EditAsync(
                editBatch => textView.Document.AsEditable(editBatch).Replace(blockRange, newText),
                cancellationToken);
        }
        catch (Exception ex)
        {
            //Debug.WriteLine($"[Proxima.Align] Error: {ex}");
        }
    }
}