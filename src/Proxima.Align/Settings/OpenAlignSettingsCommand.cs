using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

namespace Proxima.Align;

[VisualStudioContribution]
internal sealed class OpenAlignSettingsCommand : Command
{
    private readonly AlignSettingsService _settingsService;

    private static readonly CommandConfiguration _commandConfiguration =
        new("%Proxima.Align.OpenAlignSettingsCommand.DisplayName%")
        {
            // Solo Tools menu come placement diretto; Extensions viene da ProximaAlignMenu.Children
            Placements = [CommandPlacement.KnownPlacements.ToolsMenu],
            Icon       = new(ImageMoniker.KnownValues.Settings, IconSettings.IconAndText),
        };

    public override CommandConfiguration CommandConfiguration => _commandConfiguration;

    public OpenAlignSettingsCommand(AlignSettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public override async Task ExecuteCommandAsync(IClientContext context, CancellationToken cancellationToken)
    {
        var viewModel = new AlignSettingsViewModel(_settingsService);
        var control   = new AlignSettingsControl(viewModel);

        // ShowDialogAsync apre la RemoteUserControl come dialogo modale
        // che si chiude automaticamente quando l'utente clicca Save o la X
        await this.Extensibility.Shell().ShowDialogAsync(control, cancellationToken);
    }
}
