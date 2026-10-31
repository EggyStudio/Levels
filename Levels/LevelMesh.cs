using System.Numerics;

namespace Levels;

/// <summary>
/// One material's share of an entity's brushes as triangles, in meters with Y up about a place,
/// which an engine builds a mesh of to draw with the material of the same name.
/// </summary>
/// <param name="Material">The faces' material, as the level file names it.</param>
/// <param name="Positions">A corner of a face each, a face's corners its own.</param>
/// <param name="Normals">The way out of the face each corner belongs to.</param>
/// <param name="Uvs">Where each corner falls on the material's picture, its texels along the face's axes over the picture's size.</param>
/// <param name="Indices">Three corners a triangle, counter-clockwise seen from outside the brush.</param>
/// <remarks>
/// A face's corners are its own and not shared with the faces beside it, so each corner carries
/// its face's normal and its face's texture coordinates, as a brush's flat faces are drawn. A
/// face is cut into triangles from its first corner, which a convex face allows. The winding is
/// counter-clockwise seen from outside, the front faces of an engine with Y up and right-handed
/// axes, and an engine that culls the other side takes the triangles' corners in the other order.
/// </remarks>
public sealed record LevelMesh(string Material, Vector3[] Positions, Vector3[] Normals, Vector2[] Uvs, uint[] Indices);
