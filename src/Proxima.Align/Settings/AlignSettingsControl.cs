using Microsoft.VisualStudio.Extensibility.UI;

namespace Proxima.Align;

/// <summary>
/// Concrete RemoteUserControl for the Align Settings tool window.
/// The XAML template is embedded as a resource and auto-discovered by name convention.
/// </summary>
internal sealed class AlignSettingsControl : RemoteUserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AlignSettingsControl"/> class with the specified data context.
    /// </summary>
    /// <param name="dataContext">The view model to use as the data context for this control.</param>
    public AlignSettingsControl(AlignSettingsViewModel dataContext)
        : base(dataContext)
    {
    }
}
