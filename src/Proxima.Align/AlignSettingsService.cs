using System.Text.Json;

namespace Proxima.Align;

/// <summary>
/// Provides functionality to load and save application settings for Proxima.Align.
/// Settings are persisted as JSON in the user's application data folder.
/// </summary>
internal sealed class AlignSettingsService
{
    /// <summary>
    /// The name of the settings file.
    /// </summary>
    private const string SettingsFileName = "proxima-align.settings.json";

    /// <summary>
    /// The full path to the settings file in the user's application data directory.
    /// </summary>
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Proxima.Align",
        SettingsFileName);

    /// <summary>
    /// JSON serialization options configured for indented formatting.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// The currently loaded settings instance.
    /// </summary>
    private AlignSettings _current = Load();

    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    public AlignSettings Current => _current;

    /// <summary>
    /// Loads the settings from the settings file.
    /// If the file does not exist or cannot be read, returns a new default settings instance.
    /// </summary>
    /// <returns>The loaded <see cref="AlignSettings"/> or a new instance if loading fails.</returns>
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

    /// <summary>
    /// Saves the specified settings to the settings file and updates the current settings.
    /// Creates the settings directory if it does not exist. Silently fails if the save operation fails.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
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