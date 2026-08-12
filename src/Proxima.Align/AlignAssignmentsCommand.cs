using System.Diagnostics;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Editor;
using StreamJsonRpc;

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
    /// <param name="settingsService">The service providing alignment settings such as tab size and operator preferences.</param>
    public AlignAssignmentsCommand(AlignSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Executes the alignment command on the selected text in the active editor.
    /// </summary>
    /// <param name="context">The client context providing access to the Visual Studio editor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <remarks>
    /// The command performs the following steps:
    /// <list type="number">
    /// <item>Retrieves the active text view and validates that a selection exists</item>
    /// <item>Expands the selection to include complete lines</item>
    /// <item>Detects the document's line ending format (CRLF or LF)</item>
    /// <item>Retrieves the tab size from the document settings</item>
    /// <item>Applies operator alignment using the <see cref="AlignmentService"/></item>
    /// <item>Replaces the selected text with the aligned version if changes were made</item>
    /// </list>
    /// If the selection is empty, contains no alignable operators, or an error occurs, 
    /// no changes are made to the document.
    /// </remarks>
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        try
        {
            using var textView = await GetActiveTextViewAsync(context, cancellationToken);
            if (textView is null) 
                return;

            var selection = textView.Selection;
            if (selection.IsEmpty) 
                return;

            var document  = textView.Document;
            var fullText  = document.Text.CopyToString();
            var selExtent = selection.Extent;

            // Base offset of the document (can be non-zero in VS)
            int docBase = document.Text.Start.Offset;

            // Indices in the fullText buffer (0-based relative to fullText[0])
            int selStart = selExtent.Start.Offset - docBase;
            int selEnd   = selExtent.End.Offset   - docBase;

            if (selStart < 0 || selEnd > fullText.Length || selStart >= selEnd) return;

            // Detect the type of line ending used in the document
            var lineEnding = fullText.Contains("\r\n") ? "\r\n" : "\n";

            // Search backwards for the start of the line containing selStart
            int startIdx = selStart;
            while (startIdx > 0 && fullText[startIdx - 1] != '\n')
                startIdx--;

            // If selEnd falls exactly at the start of a new line (cursor in col 0)
            // or on a line break character, go back to the last character
            // of content in the last selected line.
            int endAdj = selEnd;
            while (endAdj > startIdx)
            {
                char cur  = endAdj < fullText.Length ? fullText[endAdj]     : '\0';
                char prev = endAdj > 0               ? fullText[endAdj - 1] : '\0';

                if (cur == '\r' || cur == '\n')
                    endAdj--;          // we're on a line break → go back
                else if (prev == '\n' || prev == '\r')
                    endAdj--;          // we're at the start of a line → go back
                else
                    break;
            }

            // Search forward for the end of the line containing endAdj (excluding the line break)
            int endIdx = endAdj;
            while (endIdx < fullText.Length && fullText[endIdx] != '\r' && fullText[endIdx] != '\n')
                endIdx++;

            if (endIdx <= startIdx) return;

            // Extract clean lines (without residual \r from CRLF)
            var blockText = fullText.Substring(startIdx, endIdx - startIdx);
            var lines     = blockText.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

            LogMessage($"[Proxima.Align] docBase={docBase} selStart={selStart} selEnd={selEnd}");
            LogMessage($"[Proxima.Align] startIdx={startIdx} endIdx={endIdx} lines={lines.Length}");

            for (int i = 0; i < lines.Length; i++)
                LogMessage($"[Proxima.Align]   line[{i}]: '{lines[i]}'");

            // Read the tab size from the document (default 4)
            int tabSize = 4;
            try
            {
                var tabOpt = await document.GetEditorOptionValueAsync(
                    new TextDocumentOption<int>("tab_size"), cancellationToken);
                tabSize = tabOpt.ValueOrDefault(4);
            }
            catch { /* use the default */ }

            var settings = _settingsService.Current.WithTabSize(tabSize);

            var precedingText = fullText[..startIdx];
            var aligned = AlignmentService.AlignOperators(lines, settings, precedingText);

            if (aligned is null)
            {
                LogMessage($"[Proxima.Align] AlignOperators returned null (< 2 lines with operators?)");
                return;
            }

            LogMessage($"[Proxima.Align] aligned:");

            for (int i = 0; i < aligned.Length; i++)
                LogMessage($"[Proxima.Align]   aligned[{i}]: '{aligned[i]}'");

            var newText      = string.Join(lineEnding, aligned);
            var originalText = string.Join(lineEnding, lines);
            if (newText == originalText) return;

            // Construct the TextRange using absolute positions (docBase + index)
            var rangeStart = document.Text.Start + startIdx;
            var rangeEnd   = document.Text.Start + endIdx;
            var blockRange = new TextRange(rangeStart, rangeEnd);

            await this.Extensibility.Editor().EditAsync(
                editBatch => textView.Document.AsEditable(editBatch).Replace(blockRange, newText),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The command was cancelled while Visual Studio was resolving or editing the document.
        }
        catch (Exception ex)
        {
            LogMessage($"[Proxima.Align] Error: {ex}");
        }
    }

    /// <summary>
    /// Attempts to retrieve the active text view from the Visual Studio editor context.
    /// </summary>
    /// <param name="context">The client context representing the Visual Studio editor.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The active text view snapshot, or null if it cannot be retrieved.</returns>
    private async Task<ITextViewSnapshot?> GetActiveTextViewAsync(IClientContext context,
                                                                         CancellationToken cancellationToken)
    {
        try
        {
            return await context.GetActiveTextViewAsync(cancellationToken);
        }
        catch (RemoteInvocationException ex) when (IsStaleEditorContext(ex.Message))
        {
            LogMessage("[Proxima.Align] The active editor changed before its document could be opened.");
            return null;
        }
        catch (ArgumentException ex) when (IsStaleEditorContext(ex.Message))
        {
            LogMessage("[Proxima.Align] The active document version is no longer available.");
            return null;
        }
        catch (InvalidOperationException ex) when (IsStaleEditorContext(ex.Message))
        {
            LogMessage("[Proxima.Align] The active document closed before it could be opened.");
            return null;
        }
    }

    /// <summary>
    /// Determines whether the provided error message indicates that the editor context is stale or invalid.
    /// </summary>
    /// <param name="message">The error message to evaluate.</param>
    /// <returns><c>true</c> if the message indicates a stale or invalid editor context; otherwise, <c>false</c>.</returns>
    private static bool IsStaleEditorContext(string message)
        => message.Contains("Cannot subscribe to document, document is not open",
                            StringComparison.OrdinalIgnoreCase)
        || message.Contains("Document version is no longer available",
                            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Logs a message to the Visual Studio debug output and optionally appends it to a log file in the user's application data directory.
    /// </summary>
    /// <param name="message">The message to log.</param>
    private void LogMessage(string message)
    {
        Debug.WriteLine(message);

        if (!_settingsService.Current.EnableLog) return;

        _ = Task.Run(() =>
        {
            try
            {
                var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Proxima.Align");
                Directory.CreateDirectory(logDir);
                var logFile = Path.Combine(logDir, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
                var logEntry = $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}";
                File.AppendAllText(logFile, logEntry);
            }
            catch { }
        });
    }
}