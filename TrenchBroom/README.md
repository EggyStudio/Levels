# TrenchBroom

The game config and the entity classes that let TrenchBroom draw a level of the Valve 220 map format
the module reads. An engine copies this folder into TrenchBroom's games folder, which its
preferences name, under the engine's own name, adds a file of its own classes beside `Levels.fgd`
and names it in `GameConfig.cfg`'s `definitions`, and gives TrenchBroom a game path holding a
`textures` folder, the engine's pictures of its materials.

A new face takes a scale of a sixteenth, so a picture of 512 pixels spans 32 units, a meter at the
units a meter a level takes when its `worldspawn` names none (`_units_per_meter`), and the brush
tool's 8, 16 and 32 unit grid ends on a meter's quarters. A level is saved in the Valve map format,
which is the only one offered.
