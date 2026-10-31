namespace Levels;

/// <summary>
/// What an engine says of a level's materials, the size of the picture each is drawn with, which a
/// face's texture coordinates are worked out over (N 2.1).
/// </summary>
/// <remarks>
/// The module does not know where an engine keeps its pictures or how it names them, so the engine
/// that binds it answers for a material's picture, by its own folders and its own prototypes.
/// </remarks>
public interface ILevelMaterials
{
    /// <summary>
    /// How many texels the picture of a material spans each way, or nothing where the engine has
    /// none for it, which <see cref="LevelGeometry.Meshes"/> then takes as the level's units a meter,
    /// so a face at a scale of one repeats its material once a meter.
    /// </summary>
    /// <param name="material">The material's name, as the level file writes it.</param>
    (double Width, double Height)? PictureSize(string material);
}
