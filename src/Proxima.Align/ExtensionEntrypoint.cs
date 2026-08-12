using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace Proxima.Align;

/// <summary>
/// Extension entrypoint for the Proxima Align Assignments Visual Studio extension.
/// This class serves as the main entry point for the VisualStudio.Extensibility extension framework,
/// configuring the extension's metadata and initializing required services.
/// </summary>
/// <remarks>
/// This extension provides functionality to align assignment operators (=, +=, -= , *=, /=, =>) 
/// in selected code blocks, improving code readability and consistency. The extension integrates
/// with Visual Studio through the VisualStudio.Extensibility framework.
/// </remarks>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <summary>
    /// Gets the configuration settings for the Proxima Align Assignments extension.
    /// </summary>
    /// <value>
    /// An <see cref="ExtensionConfiguration"/> instance containing the extension's metadata
    /// including its unique identifier, version, publisher information, and description.
    /// </value>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
                id:             "Proxima.Align.02d9493a-1406-4d2a-aa3b-2d686783003e",
                version:        this.ExtensionAssemblyVersion,
                publisherName:  "proxima-software",
                displayName:    "Proxima Align Assignments",
                description:    "Aligns assignment operators (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=, =>) in selected code blocks.")
        {
            Preview      = false,
            MoreInfo     = "https://github.com/MircoVanini/Proxima.Align",
            License      = "LICENSE.txt",
            ReleaseNotes = "RELEASE-NOTES.txt",
            Icon         = "Assets/icon.png",
            PreviewImage = "Assets/preview.png",
            Tags         = ["alignment", "assignments", "formatting", "productivity"],
        },
    };

    /// <summary>
    /// Initializes the dependency injection services required by the extension.
    /// </summary>
    /// <param name="serviceCollection">The service collection to register services with.</param>
    /// <remarks>
    /// This method registers the <see cref="AlignSettingsService"/> as a singleton service,
    /// which manages the configuration and settings for the alignment functionality.
    /// </remarks>
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);

        serviceCollection.AddSingleton<AlignSettingsService>();
    }
}
