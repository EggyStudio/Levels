using System.Text.Json;
using Xunit;

namespace Levels.Tests;

/// <summary>
/// TrenchBroom's game config and the base entity classes, held to the format the module reads, so
/// a level TrenchBroom saves with them is one the module reads at the scale it was drawn at.
/// </summary>
public sealed class TrenchBroomTests
{
    private static string Folder
    {
        get
        {
            // The repository's root is the folder above the tests that holds the solution.
            var at = new DirectoryInfo(AppContext.BaseDirectory);
            while (at is not null && !File.Exists(Path.Combine(at.FullName, "Levels.slnx"))) at = at.Parent;
            return Path.Combine(at?.FullName ?? throw new DirectoryNotFoundException("No Levels.slnx above the tests."), "TrenchBroom");
        }
    }

    [Fact]
    public void TheGameConfigSavesTheValveFormatAndANewFaceAtTheScaleAMeterSpans()
    {
        using var config = JsonDocument.Parse(File.ReadAllText(Path.Combine(Folder, "GameConfig.cfg")));
        var root = config.RootElement;
        Assert.Equal(["Valve"], root.GetProperty("fileformats").EnumerateArray().Select(format => format.GetProperty("format").GetString()));
        Assert.Equal(["Levels.fgd"], root.GetProperty("entities").GetProperty("definitions").EnumerateArray().Select(file => file.GetString()));

        // A sixteenth, so a picture of 512 texels spans the default units a meter.
        var scale = root.GetProperty("faceattribs").GetProperty("defaults").GetProperty("scale").EnumerateArray().Select(value => value.GetDouble()).ToArray();
        Assert.Equal([1.0 / 16, 1.0 / 16], scale);
        Assert.Equal(Level.DefaultUnitsPerMeter, 512 * scale[0]);
    }

    [Fact]
    public void TheBaseClassesAreTheWorldWithItsUnitsAMeterAndThePlayersStart()
    {
        var fgd = File.ReadAllText(Path.Combine(Folder, "Levels.fgd"));
        Assert.Contains("@SolidClass = worldspawn", fgd);
        Assert.Contains($"{Level.UnitsPerMeterKey}(float)", fgd);
        Assert.Contains($": {Level.DefaultUnitsPerMeter.ToString(System.Globalization.CultureInfo.InvariantCulture)} :", fgd);
        Assert.Contains("= info_player_start", fgd);
    }
}
