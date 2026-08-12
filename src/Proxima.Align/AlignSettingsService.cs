using System.Diagnostics;
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
    private static readonly string DefaultSettingsFilePath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "Proxima.Align",
                     SettingsFileName);

    /// <summary>
    /// JSON serialization options configured for indented formatting.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// The currently loaded settings instance.
    /// </summary>
    private readonly string _settingsFilePath;

    /// <summary>
    /// The current settings instance, which may be updated by the Save method.
    /// </summary>
    private volatile AlignSettings _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignSettingsService"/> class,
    /// </summary>
    public AlignSettingsService() : this(DefaultSettingsFilePath)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlignSettingsService"/> class with a specified settings file path.
    /// </summary>
    /// <param name="settingsFilePath">The path to the settings file.</param>
    internal AlignSettingsService(string settingsFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(settingsFilePath);

        _settingsFilePath = settingsFilePath;
        _current = Load();
    }

    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    public AlignSettings Current => _current.Copy();

    /// <summary>
    /// Loads the settings from the settings file.
    /// If the file does not exist or cannot be read, returns a new default settings instance.
    /// </summary>
    /// <returns>The loaded <see cref="AlignSettings"/> or a new instance if loading fails.</returns>
    private AlignSettings Load()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                return JsonSerializer.Deserialize<AlignSettings>(json) ?? new AlignSettings();
            }
        }
        catch (IOException ex)
        {
            Debug.WriteLine($"[Proxima.Align] Unable to read settings: {ex}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"[Proxima.Align] Unable to access settings: {ex}");
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[Proxima.Align] Invalid settings JSON: {ex}");
        }

        return new AlignSettings();
    }

    /// <summary>
    /// Saves the specified settings to the settings file and updates the current settings.
    /// Creates the settings directory if it does not exist.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    public void Save(AlignSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        Interlocked.Exchange(ref _current, settings);

        try
        {
            var settingsDirectory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(settingsDirectory))
                Directory.CreateDirectory(settingsDirectory);

            File.WriteAllText(_settingsFilePath, JsonSerializer.Serialize(settings, JsonOptions));
        }
        catch 
        { 
            System.Diagnostics.Debug.WriteLine($"[Proxima.Align] Unable to save settings to {_settingsFilePath}");
        }
    }
}