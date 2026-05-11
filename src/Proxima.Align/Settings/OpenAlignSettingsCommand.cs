using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

namespace Proxima.Align;

/// <summary>
/// Command that opens the alignment settings dialog for the Proxima.Align extension.
/// </summary>
/// <remarks>
/// This command is registered in the Visual Studio Tools menu and displays a dialog
/// allowing users to configure alignment settings through the <see cref="AlignSettingsService"/>.
/// </remarks>
[VisualStudioContribution]
internal sealed class OpenAlignSettingsCommand : Command
{
    /// <summary>
    /// The service responsible for managing alignment settings.
    /// </summary>
    private readonly AlignSettingsService _settingsService;

    /// <summary>
    /// Defines the command's configuration including display name, placement, icon, and shortcuts.
    /// </summary>
    private static readonly CommandConfiguration _commandConfiguration =
        new("%Proxima.Align.OpenAlignSettingsCommand.DisplayName%")
        {
            Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
            Icon       = new(ImageMoniker.KnownValues.Settings, IconSettings.IconAndText),
            Shortcuts  = null,
        };

    /// <summary>
    /// Gets the configuration for this command.
    /// </summary>
    /// <value>
    /// The command configuration containing display name, placement, and icon settings.
    /// </value>
    public override CommandConfiguration CommandConfiguration => _commandConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAlignSettingsCommand"/> class.
    /// </summary>
    /// <param name="settingsService">The service used to manage alignment settings.</param>
    public OpenAlignSettingsCommand(AlignSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    /// <summary>
    /// Executes the command asynchronously by displaying the alignment settings dialog.
    /// </summary>
    /// <param name="context">The client context for the command execution.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var viewModel = new AlignSettingsViewModel(_settingsService);
        var control   = new AlignSettingsControl(viewModel);

        await this.Extensibility.Shell().ShowDialogAsync(control, cancellationToken);
    }
}
