using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Editor;
using StreamJsonRpc;
using System.Diagnostics;
// ✅ rimosso: using Microsoft.VisualStudio.Settings; (non utilizzo

namespace Proxima.Align;

/// <summary>
/// Visual Studio command that aligns assignment operators and other operators 
/// in the selected text to improve code readability and formatting consistency.
/// </summary>
/// <remarks>
/// This command can be triggered via keyboard shortcut (Ctrl+Alt+\) or through 
/// the command palette. It operates on the currently selected text in the active 
/// editor, aligning operators across multiple lines while preserving indentation.
/// </remarks>
[VisualStudioContribution]
internal class AlignAssignmentsCommand : Command
{
    private static readonly CommandConfiguration _commandConfiguration =
        new("%Proxima.Align.AlignAssignmentsCommand.DisplayName%")
        {
            Placements  = [],
            Icon        = new(ImageMoniker.KnownValues.AlignLeft, IconSettings.IconAndText),
            Shortcuts   = [new CommandShortcutConfiguration(ModifierKey.ControlLeftAlt, Key.VK_OEM_5)],
            EnabledWhen = ActivationConstraint.EditorContentType("code"),
        };

    public override CommandConfiguration CommandConfiguration => _commandConfiguration;

    private readonly AlignSettingsService _settingsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignAssignmentsCommand"/> class.
    /// </summary>
    /// <param name="settingsService">The service providing alignment settings.</param>
    public AlignAssignmentsCommand(AlignSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Executes the alignment command on the selected text in the active editor.
    /// </summary>
    /// <param name="context">The client context providing access to the Visual Studio editor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        try
        {
            ITextViewSnapshot? textView;
            try
            {
                textView = await context.GetActiveTextViewAsync(cancellationToken);
            }
            catch (RemoteInvocationException ex) when (ex.Message.Contains("document is not open"))
            {
                // Race condition: documento chiuso prima che la sottoscrizione asincrona completasse.
                LogMessage("[Proxima.Align] Skipped: document closed before subscription (transient).");
                return;
            }

            if (textView is null) return;

            var selection = textView.Selection;
            if (selection.IsEmpty) return;

            var document  = textView.Document;
            var fullText  = document.Text.CopyToString();
            var selExtent = selection.Extent;

            int docBase  = document.Text.Start.Offset;
            int selStart = selExtent.Start.Offset - docBase;
            int selEnd   = selExtent.End.Offset   - docBase;

            if (selStart < 0 || selEnd > fullText.Length || selStart >= selEnd) return;

            var lineEnding = fullText.Contains("\r\n") ? "\r\n" : "\n";

            // Espande la selezione all'intera prima riga
            int startIdx = selStart;
            while (startIdx > 0 && fullText[startIdx - 1] != '\n')
                startIdx--;

            // Ritira selEnd fino all'ultimo carattere di contenuto dell'ultima riga selezionata
            int endAdj = selEnd;
            while (endAdj > startIdx)
            {
                char cur  = endAdj < fullText.Length ? fullText[endAdj]     : '\0';
                char prev = endAdj > 0               ? fullText[endAdj - 1] : '\0';

                if (cur == '\r' || cur == '\n')
                    endAdj--;
                else if (prev == '\n' || prev == '\r')
                    endAdj--;
                else
                    break;
            }

            // Avanza fino alla fine della riga contenente endAdj
            int endIdx = endAdj;
            while (endIdx < fullText.Length && fullText[endIdx] != '\r' && fullText[endIdx] != '\n')
                endIdx++;

            if (endIdx <= startIdx) return;

            var lines = fullText
                .Substring(startIdx, endIdx - startIdx)
                .Split('\n')
                .Select(l => l.TrimEnd('\r'))
                .ToArray();

            LogMessage($"[Proxima.Align] docBase={docBase} selStart={selStart} selEnd={selEnd}");
            LogMessage($"[Proxima.Align] startIdx={startIdx} endIdx={endIdx} lines={lines.Length}");
            for (int i = 0; i < lines.Length; i++)
                LogMessage($"[Proxima.Align]   line[{i}]: '{lines[i]}'");

            int tabSize = 4;
            try
            {
                var tabOpt = await document.GetEditorOptionValueAsync(
                    new TextDocumentOption<int>("tab_size"), cancellationToken);
                tabSize = tabOpt.ValueOrDefault(4);
            }
            catch { /* use the default */ }

            var settings = _settingsService.Current.WithTabSize(tabSize);
            var aligned  = AlignmentService.AlignOperators(lines, settings);

            if (aligned is null)
            {
                LogMessage("[Proxima.Align] AlignOperators returned null (< 2 lines with operators?)");
                return;
            }

            LogMessage("[Proxima.Align] aligned:");
            for (int i = 0; i < aligned.Length; i++)
                LogMessage($"[Proxima.Align]   aligned[{i}]: '{aligned[i]}'");

            var newText      = string.Join(lineEnding, aligned);
            var originalText = string.Join(lineEnding, lines);
            if (newText == originalText) return;

            var blockRange = new TextRange(document.Text.Start + startIdx, document.Text.Start + endIdx);

            await this.Extensibility.Editor().EditAsync(
                editBatch => textView.Document.AsEditable(editBatch).Replace(blockRange, newText),
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cancellazione attesa, nessuna azione
        }
        catch (Exception ex)
        {
            LogMessage($"[Proxima.Align] Error: {ex}");
        }
    }

    /// <summary>
    /// Logs a diagnostic message to the debug output and, if logging is enabled,
    /// appends it asynchronously (fire-and-forget) to the daily log file.
    /// </summary>
    private void LogMessage(string message)
    {
        Debug.WriteLine(message);

        if (!_settingsService.Current.EnableLog) return;

        _ = Task.Run(() =>
        {
            try
            {
                var logDir   = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Proxima.Align");
                Directory.CreateDirectory(logDir);
                var logFile  = Path.Combine(logDir, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                var logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logEntry);
            }
            catch { /* silent */ }
        });
    }
}