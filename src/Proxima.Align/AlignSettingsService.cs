using System.Text.Json;

namespace Proxima.Align;

/// <summary>
/// Provides functionality to load and save application settings for Proxima.Align.
/// Settings are persisted as JSON in the user's application data folder.
/// Thread-safe: reads and writes to <see cref="Current"/> are atomic.
/// </summary>
internal sealed class AlignSettingsService
{
    private const string SettingsFileName = "proxima-align.settings.json";

    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Proxima.Align",
        SettingsFileName);

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // volatile garantisce visibilità cross-thread senza lock per le semplici letture
    private volatile AlignSettings _current = Load();

    /// <summary>
    /// Gets the current application settings.
    /// Always returns the latest snapshot; never mutate the returned instance directly.
    /// </summary>
    public AlignSettings Current => _current;

    /// <summary>
    /// Loads the settings from the settings file.
    /// If the file does not exist or cannot be read, returns a new default settings instance.
    /// </summary>
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
    /// Saves the specified settings to the settings file and atomically updates <see cref="Current"/>.
    /// Creates the settings directory if it does not exist. Silently fails if the save operation fails.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    public void Save(AlignSettings settings)
    {
        // Swap atomico: nessun lock necessario per la lettura di Current
        Interlocked.Exchange(ref _current, settings);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch { }
    }
}