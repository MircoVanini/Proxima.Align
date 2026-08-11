using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace Proxima.Align;

/// <summary>
/// Extension entrypoint for the Proxima Align Assignments Visual Studio extension.
/// </summary>
/// <remarks>
/// This extension aligns assignment operators
/// (=, +=, -=, *=, /=, %=, &amp;=, |=, ^=, &lt;&lt;=, &gt;&gt;=, =&gt;)
/// in selected code blocks, improving readability and formatting consistency.
/// </remarks>
[VisualStudioContribution]
internal class ExtensionEntrypoint : Extension
{
    /// <inheritdoc/>
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id:            "Proxima.Align.02d9493a-1406-4d2a-aa3b-2d686783003e",
            version:       this.ExtensionAssemblyVersion,
            publisherName: "Proxima Software",
            displayName:   "Proxima Align Assignments",
            description:   "Aligns assignment operators (=, +=, -=, *=, /=, %=, &=, |=, ^=, <<=, >>=, =>) in selected code blocks."),
    };

    /// <inheritdoc/>
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
        serviceCollection.AddSingleton<AlignSettingsService>();
    }
}
