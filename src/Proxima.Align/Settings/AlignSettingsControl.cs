using Microsoft.VisualStudio.Extensibility.UI;

namespace Proxima.Align;

/// <summary>
/// Concrete RemoteUserControl for the Align Settings tool window.
/// The XAML template is embedded as a resource and auto-discovered by name convention.
/// </summary>
internal sealed class AlignSettingsControl : RemoteUserControl
{
    public AlignSettingsControl(AlignSettingsViewModel dataContext)
        : base(dataContext)
    {
    }
}
