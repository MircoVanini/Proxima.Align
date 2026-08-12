using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.Win32;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System.Text;

namespace Proxima.Align;

/// <summary>
/// Represents the settings tool window for the Align Assignments extension.
/// Provides a user interface for configuring alignment settings.
/// </summary>
[VisualStudioContribution]
internal sealed class AlignSettingsToolWindow : ToolWindow
{
    private readonly AlignSettingsViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignSettingsToolWindow"/> class.
    /// </summary>
    /// <param name="settingsService">The settings service used to manage alignment settings.</param>
    public AlignSettingsToolWindow(AlignSettingsService settingsService)
    {
        _viewModel = new AlignSettingsViewModel(settingsService);
        Title      = "Align Assignments – Settings";
    }

    /// <summary>
    /// Gets the configuration for this tool window.
    /// Configures the window to be displayed as a floating window.
    /// </summary>
    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.Floating,
    };

    /// <summary>
    /// Gets the content control for the tool window asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the remote user control for the tool window.</returns>
    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => Task.FromResult<IRemoteUserControl>(new AlignSettingsControl(_viewModel));
}
