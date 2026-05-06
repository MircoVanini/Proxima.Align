using System.Text.Json;

namespace Proxima.Align;

internal sealed class AlignSettingsService
{
    private const string SettingsFileName = "proxima-align.settings.json";

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Proxima.Align",
        SettingsFileName);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private AlignSettings _current = Load();

    public AlignSettings Current => _current;

    private static AlignSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AlignSettings>(json) ?? new AlignSettings();
            }
        }
        catch { }
        return new AlignSettings();
    }

    public void Save(AlignSettings settings)
    {
        _current = settings;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch { }
    }
}