using System.Buffers.Binary;
using System.Numerics;

namespace Levels;

/// <summary>
/// An entity's brushes as the triangles an engine draws, a mesh a material, and as the convex hulls
/// it collides with, in meters with Y up about a place, and the texel math and the pictures' sizes
/// they are made with.
/// </summary>
/// <remarks>
/// <para>
/// A brush's faces are the polygons where its planes meet (<see cref="LevelBrush.Polygons"/>), each
/// wound clockwise seen from outside as the file winds them, and taken backwards here, so they run
/// counter-clockwise from outside once their points are turned into meters with Y up. A face's
/// texture coordinates are its points along its axes, scaled and moved as the file says, over the
/// texels its material's picture spans, so a material stays aligned to the world across every face
/// it is on, as TrenchBroom aligns it.
/// </para>
/// <para>
/// What the module gives out is arrays and not an engine's types, and the engine that binds it
/// builds its own meshes and bodies from them and answers for its materials' pictures through
/// <see cref="ILevelMaterials"/> (N 2.1).
/// </para>
/// </remarks>
public static class LevelGeometry
{
    /// <summary>An entity's brushes as triangles, a mesh each material in the order the faces first name them.</summary>
    /// <param name="source">The entity, the world's brushes or a brush entity's.</param>
    /// <param name="unitsPerMeter">How many of the file's units make a meter, <see cref="Level.UnitsPerMeter"/>.</param>
    /// <param name="origin">The place in meters the corners are given about, the entity's own place for a brush entity.</param>
    /// <param name="materials">The engine's pictures, each material's size in texels, the level's units a meter where none is given.</param>
    /// <returns>The meshes, none for an entity with no brushes.</returns>
    public static IReadOnlyList<LevelMesh> Meshes(LevelEntity source, double unitsPerMeter, Vector3 origin, ILevelMaterials? materials = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        var built = new Dictionary<string, (List<Vector3> Positions, List<Vector3> Normals, List<Vector2> Uvs, List<uint> Indices)>(StringComparer.Ordinal);
        var order = new List<string>();
        var sizes = new Dictionary<string, (double Width, double Height)>(StringComparer.Ordinal);
        (double Width, double Height) SizeOf(string material) =>
            sizes.TryGetValue(material, out var known) ? known : sizes[material] = materials?.PictureSize(material) ?? (unitsPerMeter, unitsPerMeter);

        foreach (var brush in source.Brushes)
        {
            var polygons = brush.Polygons();
            for (var f = 0; f < brush.Faces.Count; f++)
            {
                if (polygons[f] is not { } polygon) continue;
                var face = brush.Faces[f];
                if (!built.TryGetValue(face.Material, out var mesh))
                {
                    built[face.Material] = mesh = ([], [], [], []);
                    order.Add(face.Material);
                }

                var (width, height) = SizeOf(face.Material);
                var normal = Vector3.Normalize(face.Normal.Meters(1));
                var first = (uint)mesh.Positions.Count;
                for (var i = 0; i < polygon.Length; i++)
                {
                    var point = polygon[polygon.Length - 1 - i];
                    mesh.Positions.Add(point.Meters(unitsPerMeter) - origin);
                    mesh.Normals.Add(normal);
                    mesh.Uvs.Add(Texel(face, point, width, height));
                }

                // A convex face is a fan of triangles from its first corner.
                for (var i = 1; i + 1 < polygon.Length; i++)
                {
                    mesh.Indices.Add(first);
                    mesh.Indices.Add(first + (uint)i);
                    mesh.Indices.Add(first + (uint)i + 1);
                }
            }
        }

        return [.. order.Select(material => built[material]).Select((mesh, at) =>
            new LevelMesh(order[at], [.. mesh.Positions], [.. mesh.Normals], [.. mesh.Uvs], [.. mesh.Indices]))];
    }

    /// <summary>An entity's brushes as convex hulls, a hull each brush with a solid.</summary>
    /// <param name="source">The entity, the world's brushes or a brush entity's.</param>
    /// <param name="unitsPerMeter">How many of the file's units make a meter.</param>
    /// <param name="origin">The place in meters the corners are given about.</param>
    /// <returns>The hulls, a brush whose planes enclose nothing left out.</returns>
    public static IReadOnlyList<LevelHull> Hulls(LevelEntity source, double unitsPerMeter, Vector3 origin)
    {
        ArgumentNullException.ThrowIfNull(source);
        var hulls = new List<LevelHull>();
        foreach (var brush in source.Brushes)
        {
            var corners = brush.Polygons().OfType<LevelPoint[]>().SelectMany(polygon => polygon).Distinct().ToArray();
            if (corners.Length >= 4) hulls.Add(new LevelHull([.. corners.Select(corner => corner.Meters(unitsPerMeter) - origin)]));
        }

        return hulls;
    }

    /// <summary>
    /// Where a point of a face falls on its material, its texel along each axis, scaled and moved,
    /// over the texels its picture spans each way.
    /// </summary>
    /// <param name="face">The face, its axes, offsets and scales.</param>
    /// <param name="point">The point, in the file's units.</param>
    /// <param name="width">The texels the material's picture spans across.</param>
    /// <param name="height">The texels it spans down.</param>
    public static Vector2 Texel(LevelFace face, LevelPoint point, double width, double height)
    {
        ArgumentNullException.ThrowIfNull(face);
        return new((float)((LevelPoint.Dot(point, face.UAxis) / face.ScaleU + face.UOffset) / width),
            (float)((LevelPoint.Dot(point, face.VAxis) / face.ScaleV + face.VOffset) / height));
    }

    /// <summary>A PNG's or a JPEG's width and height from its header, or nothing for other bytes.</summary>
    /// <param name="bytes">The file's bytes, its header at least.</param>
    /// <remarks>Read from the header alone, so an engine sizing its pictures for a level's texels reads no picture whole.</remarks>
    public static (int Width, int Height)? PictureSize(ReadOnlySpan<byte> bytes)
    {
        // A PNG's header chunk holds its width and height as big-endian four byte numbers.
        ReadOnlySpan<byte> png = [0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A];
        if (bytes.Length >= 24 && bytes[..8].SequenceEqual(png))
            return (BinaryPrimitives.ReadInt32BigEndian(bytes[16..]), BinaryPrimitives.ReadInt32BigEndian(bytes[20..]));

        // A JPEG's size is in its start of frame, after the segments before it, each a marker and a length.
        if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8) return null;
        for (var at = 2; at + 9 < bytes.Length;)
        {
            if (bytes[at] != 0xFF) return null;
            var marker = bytes[at + 1];
            var length = bytes[at + 2] << 8 | bytes[at + 3];
            if (marker is >= 0xC0 and <= 0xCF and not (0xC4 or 0xC8 or 0xCC)) return (bytes[at + 7] << 8 | bytes[at + 8], bytes[at + 5] << 8 | bytes[at + 6]);
            at += 2 + length;
        }

        return null;
    }
}
