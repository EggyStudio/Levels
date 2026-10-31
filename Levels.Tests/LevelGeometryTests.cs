using System.Numerics;
using Xunit;

namespace Levels.Tests;

/// <summary>
/// A level entity's brushes as triangles a material a mesh and as convex hulls, in meters with Y up,
/// their texture coordinates over the pictures an engine says the materials have, and a picture's
/// size read from its header.
/// </summary>
public sealed class LevelGeometryTests
{
    /// <summary>The engine's side, a picture's size by a material's name.</summary>
    private sealed class Pictures(Dictionary<string, (double, double)> sizes) : ILevelMaterials
    {
        public (double Width, double Height)? PictureSize(string material) => sizes.TryGetValue(material, out var size) ? size : null;
    }

    [Fact]
    public void ARoomsBrushesAreMeshedAMeshAMaterialInMetersWithYUpWoundOutward()
    {
        var world = Level.Parse(LevelTests.Room).World!;
        var meshes = LevelGeometry.Meshes(world, 32, Vector3.Zero);

        // The floor from four meters each side at Y zero down to half a meter, the wall three meters
        // up on the side minus Z, which the file's plus Y is, and the ramp of the floor's material.
        Assert.Equal(["floor", "wall"], meshes.Select(mesh => mesh.Material));
        var all = meshes.SelectMany(mesh => mesh.Positions).ToArray();
        Assert.Equal(new Vector3(-4f, -0.5f, -4f), all.Aggregate(Vector3.Min));
        Assert.Equal(new Vector3(4f, 3f, 4f), all.Aggregate(Vector3.Max));

        // Two boxes of six quads and a wedge of two triangles and three quads, two triangles a quad.
        Assert.Equal((12 + 12 + 8) * 3, meshes.Sum(mesh => mesh.Indices.Length));

        // Every triangle runs counter-clockwise seen from outside, its corners' turn along its normal.
        foreach (var mesh in meshes)
        {
            for (var t = 0; t < mesh.Indices.Length; t += 3)
            {
                var (a, b, c) = (mesh.Positions[mesh.Indices[t]], mesh.Positions[mesh.Indices[t + 1]], mesh.Positions[mesh.Indices[t + 2]]);
                Assert.True(Vector3.Dot(Vector3.Cross(b - a, c - a), mesh.Normals[mesh.Indices[t]]) > 0f, $"a triangle of {mesh.Material} at {a}, {b}, {c} faces in");
            }
        }

        // The floor's top faces up, and its corners are given about a place.
        var floor = meshes[0];
        Assert.Contains(floor.Normals, normal => normal == Vector3.UnitY);
        var moved = LevelGeometry.Meshes(world, 32, new Vector3(1f, 2f, 3f));
        Assert.Equal(floor.Positions.Select(p => p - new Vector3(1f, 2f, 3f)), moved[0].Positions);
    }

    [Fact]
    public void AFacesTexelsAreItsAxesOverItsMaterialsPicture()
    {
        // At a scale of one a material repeats each meter along its axes, and a scale and an offset
        // move it as the file says.
        var world = Level.Parse(LevelTests.Room).World!;
        var floorTop = world.Brushes[0].Faces[3];
        Assert.Equal(new Vector2(2f, -1f), LevelGeometry.Texel(floorTop, new LevelPoint(64, 32, 0), 32, 32));
        Assert.Equal(new Vector2(4.5f, -1f), LevelGeometry.Texel(floorTop with { ScaleU = 0.5, UOffset = 16 }, new LevelPoint(64, 32, 0), 32, 32));

        // A picture of 512 texels at a sixteenth's scale repeats once 32 units, a meter, as the
        // level's units a meter do where the engine has no picture for a material.
        var face = floorTop with { ScaleU = 0.0625, ScaleV = 0.0625 };
        Assert.Equal(new Vector2(2f, -1f), LevelGeometry.Texel(face, new LevelPoint(64, 32, 0), 512, 512));

        // The meshes read each material's picture through the engine's answer.
        var pictures = new Pictures(new() { ["wall"] = (256, 128) });
        var wall = LevelGeometry.Meshes(world, 32, Vector3.Zero, pictures).Single(mesh => mesh.Material == "wall");
        var plain = LevelGeometry.Meshes(world, 32, Vector3.Zero).Single(mesh => mesh.Material == "wall");
        for (var i = 0; i < wall.Uvs.Length; i++) Assert.Equal(plain.Uvs[i] * new Vector2(32f / 256f, 32f / 128f), wall.Uvs[i]);
    }

    [Fact]
    public void ABrushIsAConvexHullOfItsCorners()
    {
        var world = Level.Parse(LevelTests.Room).World!;
        var hulls = LevelGeometry.Hulls(world, 32, Vector3.Zero);
        Assert.Equal(3, hulls.Count);
        Assert.Equal([8, 8, 6], hulls.Select(hull => hull.Corners.Length));
        Assert.All(hulls[0].Corners, corner => Assert.True(MathF.Abs(corner.X) == 4f && MathF.Abs(corner.Z) == 4f && (corner.Y == 0f || corner.Y == -0.5f), $"a floor corner at {corner}"));
    }

    [Fact]
    public void APicturesSizeIsReadFromItsHeader()
    {
        // A PNG's signature and its header chunk's length, name, width and height.
        byte[] png = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, (byte)'I', (byte)'H', (byte)'D', (byte)'R', 0, 0, 2, 0, 0, 0, 1, 128];
        Assert.Equal((512, 384), LevelGeometry.PictureSize(png));
        byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x01, 0x80, 0x02, 0x00, 0x03, 0x00, 0x00];
        Assert.Equal((512, 384), LevelGeometry.PictureSize(jpeg));
        Assert.Null(LevelGeometry.PictureSize([1, 2, 3]));
    }
}
