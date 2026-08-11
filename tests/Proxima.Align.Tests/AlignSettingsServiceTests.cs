using System.Text.Json;
using Xunit;

namespace Proxima.Align.Tests;

public sealed class AlignSettingsServiceTests : IDisposable
{
    private readonly string _testDirectory = Path.Combine(
        Path.GetTempPath(),
        "Proxima.Align.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void Save_UpdatesFileAndCurrentAfterSuccessfulWrite()
    {
        var settingsPath = Path.Combine(_testDirectory, "settings.json");
        var service = new AlignSettingsService(settingsPath);
        var settings = new AlignSettings
        {
            EnabledOperators = ["=", "=>"],
            AutoAlign = true,
            AlignComments = true,
            SpaceBeforeOperator = false,
            SpaceAfterOperator = false,
            TabSize = 8,
        };

        service.Save(settings);

        Assert.Same(settings, service.Current);
        var persisted = JsonSerializer.Deserialize<AlignSettings>(File.ReadAllText(settingsPath));
        Assert.NotNull(persisted);
        Assert.Equal(settings.EnabledOperators, persisted.EnabledOperators);
        Assert.Equal(settings.AutoAlign, persisted.AutoAlign);
        Assert.Equal(settings.AlignComments, persisted.AlignComments);
        Assert.Equal(settings.TabSize, persisted.TabSize);
    }

    [Fact]
    public void Save_DoesNotUpdateCurrentWhenWriteFails()
    {
        Directory.CreateDirectory(_testDirectory);
        var blockingFile = Path.Combine(_testDirectory, "blocking-file");
        File.WriteAllText(blockingFile, "content");
        var service = new AlignSettingsService(Path.Combine(blockingFile, "settings.json"));
        var original = service.Current;

        Assert.Throws<IOException>(() => service.Save(new AlignSettings { AutoAlign = true }));

        Assert.Same(original, service.Current);
        Assert.False(service.Current.AutoAlign);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, recursive: true);
    }
}
