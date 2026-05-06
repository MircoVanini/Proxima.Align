using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.ToolWindows;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.Win32;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System.Text;

namespace Proxima.Align;

[VisualStudioContribution]
internal sealed class AlignSettingsToolWindow : ToolWindow
{
    private readonly AlignSettingsViewModel _viewModel;

    public AlignSettingsToolWindow(AlignSettingsService settingsService)
    {
        _viewModel = new AlignSettingsViewModel(settingsService);
        Title = "Align Assignments – Settings";

        // DEBUG: scrive il contenuto del registro in %TEMP%\proxima-theme-debug.txt
        DumpThemeRegistry();
    }

    private static void DumpThemeRegistry()
    {
        try
        {
            var sb = new StringBuilder();
            using var vsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\VisualStudio");
            if (vsKey == null) { sb.AppendLine("(no HKCU\\Software\\Microsoft\\VisualStudio)"); }
            else
            {
                foreach (var ver in vsKey.GetSubKeyNames().OrderByDescending(n => n))
                {
                    sb.AppendLine($"=== {ver} ===");

                    // General
                    using var gen = vsKey.OpenSubKey($@"{ver}\General");
                    if (gen != null)
                        foreach (var vn in gen.GetValueNames())
                            sb.AppendLine($"  General\\{vn} = {gen.GetValue(vn)}");

                    // ApplicationPrivateSettings
                    using var app = vsKey.OpenSubKey(
                        $@"{ver}\ApplicationPrivateSettings\Microsoft\VisualStudio");
                    if (app != null)
                        foreach (var vn in app.GetValueNames())
                            sb.AppendLine($"  AppPrivate\\{vn} = {app.GetValue(vn)}");
                }
            }

            var path = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData),
                "proxima-theme-debug.txt");
            System.IO.File.WriteAllText(path, sb.ToString());
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.LocalApplicationData),
                    "proxima-theme-debug.txt"),
                ex.ToString());
        }
    }

    public override ToolWindowConfiguration ToolWindowConfiguration => new()
    {
        Placement = ToolWindowPlacement.Floating,
    };

    public override Task<IRemoteUserControl> GetContentAsync(CancellationToken cancellationToken)
        => Task.FromResult<IRemoteUserControl>(new AlignSettingsControl(_viewModel));
}
