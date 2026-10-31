using System.Globalization;
using System.Numerics;
using Xunit;

namespace Levels.Tests;

/// <summary>
/// A level file of brushes and entities in the Valve 220 map format TrenchBroom writes, read and
/// written back, its brushes' corners found where their planes meet and its points turned into
/// meters.
/// </summary>
public sealed class LevelTests
{
    // A room's floor and a wall, a ramp whose slope meets its floor along one edge, a player's
    // start, a door with two outputs of one name, and a tree, at the default 32 units a meter.
    private static readonly string Room = string.Join("\n",
        "// Game: Generic",
        "// Format: Valve",
        "// entity 0",
        "{",
        "\"mapversion\" \"220\"",
        "\"classname\" \"worldspawn\"",
        "// brush 0",
        "{",
        Box(-128, -128, -16, 128, 128, 0, "floor"),
        "}",
        "// brush 1",
        "{",
        Box(-128, 112, 0, 128, 128, 96, "wall"),
        "}",
        "// brush 2",
        "{",
        "( 0 -64 0 ) ( 0 -63 0 ) ( 0 -64 1 ) floor [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
        "( 0 -64 0 ) ( 0 -64 1 ) ( 1 -64 0 ) floor [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
        "( 0 -64 0 ) ( 1 -64 0 ) ( 0 -63 0 ) floor [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
        "( 128 -64 64 ) ( 0 -64 0 ) ( 0 64 0 ) floor [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
        "( 128 64 64 ) ( 129 64 64 ) ( 128 64 65 ) floor [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
        "( 128 64 64 ) ( 128 64 65 ) ( 128 65 64 ) floor [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
        "}",
        "}",
        "// entity 1",
        "{",
        "\"classname\" \"info_player_start\"",
        "\"origin\" \"-64 0 24\"",
        "\"angle\" \"90\"",
        "}",
        "// entity 2",
        "{",
        "\"classname\" \"func_door\"",
        "\"targetname\" \"door\"",
        "\"OnOpen\" \"lamp,TurnOn,,0,-1\"",
        "\"OnOpen\" \"chime,Play,,0.5,1\"",
        "// brush 0",
        "{",
        Box(-32, 104, 0, 32, 112, 96, "door"),
        "}",
        "}",
        "// entity 3",
        "{",
        "\"classname\" \"tree\"",
        "\"origin\" \"96 -96 0\"",
        "\"species\" \"black-tupelo\"",
        "\"seed\" \"3\"",
        "}",
        "");


    [Fact]
    public void ALevelIsReadAsItsEntitiesKeysAndBrushes()
    {
        var level = Level.Parse(Room);

        Assert.Equal("Generic", level.Game);
        Assert.Equal(["worldspawn", "info_player_start", "func_door", "tree"], level.Entities.Select(entity => entity.ClassName));
        Assert.Same(level.Entities[0], level.World);
        Assert.Equal(Level.DefaultUnitsPerMeter, level.UnitsPerMeter);
        Assert.Equal([3, 0, 1, 0], level.Entities.Select(entity => entity.Brushes.Count));
        Assert.All(level.World!.Brushes, brush => Assert.Equal(6, brush.Faces.Count));

        // Two outputs of one name kept in their order, the first answered by the indexer.
        var door = level.Entities[2];
        Assert.Equal(["lamp,TurnOn,,0,-1", "chime,Play,,0.5,1"], door.All("OnOpen"));
        Assert.Equal("lamp,TurnOn,,0,-1", door["OnOpen"]);

        var slope = level.World.Brushes[2].Faces[3];
        Assert.Equal(new LevelPoint(128, -64, 64), slope.First);
        Assert.Equal("floor", slope.Material);
        Assert.Equal((new LevelPoint(1, 0, 0), 0.0, new LevelPoint(0, -1, 0), 0.0), (slope.UAxis, slope.UOffset, slope.VAxis, slope.VOffset));
        Assert.Equal((0.0, 1.0, 1.0), (slope.Rotation, slope.ScaleU, slope.ScaleV));
    }

    [Fact]
    public void ALevelWrittenByTheEngineReadsAndWritesBackByteForByte()
    {
        Assert.Equal(Room, Level.Parse(Room).Write());

        // Numbers that are not whole, a quote in a value and a units key, made in code and written,
        // read back the same and written again the same.
        var made = new Level();
        var world = new LevelEntity();
        world["classname"] = "worldspawn";
        world[Level.UnitsPerMeterKey] = "64";
        var brush = new LevelBrush();
        brush.Faces.Add(new LevelFace(new(0.5, -1.25, 1e-7), new(3.0000001, 2, 0), new(0, 0, -7.75), "dev/grid", new(0.7071067811865476, 0.7071067811865476, 0), 12.5, new(0, 0, -1), -3, 45, 0.25, 0.0625));
        world.Brushes.Add(brush);
        made.Entities.Add(world);
        var sign = new LevelEntity();
        sign["classname"] = "info_sign";
        sign["message"] = "Mind the \"gap\"";
        made.Entities.Add(sign);

        var text = made.Write();
        var read = Level.Parse(text);
        Assert.Equal(text, read.Write());
        Assert.Equal(64, read.UnitsPerMeter);
        Assert.Equal("Mind the \"gap\"", read.Entities[1]["message"]);
        Assert.Equal(brush.Faces[0], read.World!.Brushes[0].Faces[0]);
    }

    [Fact]
    public void ALevelLaidOutAnotherWayReadsTheSameAndIsWrittenTheEnginesWay()
    {
        // Braces sharing lines, numbers written with a point and a sign, tabs, comments after a
        // key and no comments numbering anything, as a hand or another tool may write a level.
        const string other = "{ \"classname\" \"worldspawn\" // the world\n"
                             + "{\t( -128.0 -128 -16 ) ( -128 -127 -16 ) ( -128 -128 -15 ) floor [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1\n"
                             + "( -128 -128 -16 ) ( -128 -128 -15 ) ( -127 -128 -16 ) floor [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1.0 1\n"
                             + "( -128 -128 -16 ) ( -127 -128 -16 ) ( -128 -127 -16 ) floor [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1\n"
                             + "( 128 128 0 ) ( 128 129 0 ) ( 129 128 0 ) floor [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1\n"
                             + "( 128 128 0 ) ( 129 128 0 ) ( 128 128 1 ) floor [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1\n"
                             + "( 128 128 0 ) ( 128 128 1 ) ( 128 129 0 ) floor [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 +1 1 } }";
        var read = Level.Parse(other);
        var written = read.Write();

        Assert.Equal(Box(-128, -128, -16, 128, 128, 0, "floor"), string.Join("\n", written.Split('\n').Where(line => line.StartsWith('('))));
        Assert.Equal(written, Level.Parse(written).Write());
    }

    [Theory]
    [InlineData("{ \"classname\" \"worldspawn\"\n{\n( 0 0 0 ) ( 0 1 0 ) ( 0 0 1 ) floor 0 0 0 1 1\n}\n}", 3, "Quake's older format")]
    [InlineData("{ \"classname\" \"worldspawn\"\n\"wad\"\n}", 3, "quoted value is meant")]
    [InlineData("{ \"classname\" \"worldspawn\"\n{\n( 0 0 0 ) ( 0 1 0 ) ( 0 0 x ) floor [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1\n}\n}", 3, "point's z is meant as a number")]
    [InlineData("{ \"classname\" \"worldspawn\"\n{\n( 0 0 0 ) ( 0 1 0 ) ( 0 0 1 ) floor [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1 0 0 0\n}\n}", 3, "goes on with 0")]
    [InlineData("{ \"classname\" \"worldspawn\"\n", 2, "ends inside an entity")]
    public void AFileThatIsNotMapTextIsRefusedAtTheLineItFailsOn(string text, int line, string said)
    {
        var fault = Assert.Throws<FormatException>(() => Level.Parse(text, "broken.map"));
        Assert.StartsWith($"broken.map, line {line}: ", fault.Message);
        Assert.Contains(said, fault.Message);
    }

    [Fact]
    public void ABrushsCornersAreWherePlanesMeetWithinTheRest()
    {
        var world = Level.Parse(Room).World!;

        // A box, six faces of four corners, eight corners in all, each face wound clockwise seen
        // from outside, its normal the way out.
        var box = world.Brushes[0];
        var faces = box.Polygons();
        Assert.All(faces, face => Assert.Equal(4, face!.Length));
        Assert.Equal(8, faces.SelectMany(face => face!).Distinct().Count());
        for (var f = 0; f < faces.Length; f++)
        {
            var corners = faces[f]!;
            var normal = LevelPoint.Cross(corners[0] - corners[1], corners[2] - corners[1]);
            Assert.True(LevelPoint.Dot(normal, box.Faces[f].Normal) > 0, $"face {f} is wound against its plane");
        }

        // The ramp's back face only touches it along the edge its slope meets its floor at, and is
        // no polygon; the slope and its floor are four corners, its sides three.
        var ramp = world.Brushes[2].Polygons();
        Assert.Null(ramp[0]);
        Assert.Equal([3, 3, 4, 4, 4], ramp.Skip(1).Select(face => face!.Length).Order());

        // Planes that close nothing are refused.
        var open = new LevelBrush();
        open.Faces.AddRange(box.Faces.Take(5));
        Assert.Throws<InvalidOperationException>(() => open.Polygons());
    }

    [Fact]
    public void ALevelsUnitsAreTurnedIntoMetersWithYUp()
    {
        // The file's (x, y, z) at (x, z, -y) over its units a meter, a turn keeping a face's winding.
        Assert.Equal(new Vector3(2f, 3f, -1f), new LevelPoint(64, 32, 96).Meters(32));
        Assert.Equal(new LevelPoint(64, 32, 96), LevelPoint.FromMeters(new Vector3(2f, 3f, -1f), 32));
    }

    /// <summary>An axis-aligned box brush's six faces, as TrenchBroom writes one.</summary>
    private static string Box(int x1, int y1, int z1, int x2, int y2, int z2, string material)
    {
        static string P(int x, int y, int z) => $"( {x.ToString(CultureInfo.InvariantCulture)} {y.ToString(CultureInfo.InvariantCulture)} {z.ToString(CultureInfo.InvariantCulture)} )";
        return string.Join("\n",
            $"{P(x1, y1, z1)} {P(x1, y1 + 1, z1)} {P(x1, y1, z1 + 1)} {material} [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1",
            $"{P(x1, y1, z1)} {P(x1, y1, z1 + 1)} {P(x1 + 1, y1, z1)} {material} [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
            $"{P(x1, y1, z1)} {P(x1 + 1, y1, z1)} {P(x1, y1 + 1, z1)} {material} [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
            $"{P(x2, y2, z2)} {P(x2, y2 + 1, z2)} {P(x2 + 1, y2, z2)} {material} [ 1 0 0 0 ] [ 0 -1 0 0 ] 0 1 1",
            $"{P(x2, y2, z2)} {P(x2 + 1, y2, z2)} {P(x2, y2, z2 + 1)} {material} [ 1 0 0 0 ] [ 0 0 -1 0 ] 0 1 1",
            $"{P(x2, y2, z2)} {P(x2, y2, z2 + 1)} {P(x2, y2 + 1, z2)} {material} [ 0 1 0 0 ] [ 0 0 -1 0 ] 0 1 1");
    }
}
