using System.Numerics;

namespace Levels;

/// <summary>
/// A brush's solid as its corners, in meters with Y up about a place, which an engine makes a
/// convex collision shape of.
/// </summary>
/// <param name="Corners">Each corner of the brush once, where three of its planes or more meet.</param>
/// <remarks>
/// A brush is convex by the format's own rule, so the hull of its corners is the brush, and a
/// physics engine that builds a convex shape from points takes them as they are, a brush a shape,
/// where a level's triangles as one mesh would collide only on their faces and not inside them.
/// </remarks>
public sealed record LevelHull(Vector3[] Corners);
