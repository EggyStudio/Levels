# Review

Reviewed up to `3ca2ac0`, item 2, pushed at 12:31 on 2026-10-31 with `751c272`, item 1, after the
owner's first commit, its run green at 12:35 on the three systems in about three minutes, the first
of this repository. Both commits keep the form, the three-mark subject, one sentence, the owner's
identity as author and committer, no trailer (N 7.2 and N 7.6 kept at the first commit); the library
references no engine (N 2.1). The documents are the engines' with the engine's name replaced and the
engine-only lines dropped, COMMITS.md the game's with N 7.1's pointer rule in place of the legacy
branch line, .gitignore with the dump folder, .claude and .ref; AGENTS.md's devlog row carries no
link until DEVLOG.md exists and its table of areas gains the rows item 2 adds. The skeleton sets
ImportDirectoryBuildTargets false, since a submodule under modules/ would inherit an engine's
Directory.Build.targets, makes a missing summary an error, loads the assembly by name in its one
test, and build/version.sh prints 0.1.0; the strict build 0 warnings and 0 errors, the test passing.
Before it `b40fa1d`, the license alone, the owner's.

## Now

The order the owner set on 2026-10-31 for the six modules, this one holding the Valve 220 map
format's reader, the brushes to meshes and collision, and the TrenchBroom config and fgd. The
documents first, then the rest as the other projects have it, then the engines taking the module.
Items 1 and 2 are the game's session's, being free while the game waits on the engine's package;
item 3 is the session's that owns the code today; item 4 is each engine's session's. Among the six
this module comes third, after Modeling, on Modeling.

1. **The documents, one commit.** AGENTS.md on the game's shape (one paragraph saying what the
    module is and that no engine is referenced, N 2.1; the documents table with STYLE, COMMITS,
    REVIEW, NORM and DEVLOG; building and testing; Where things are with a row a folder; the
    conventions, the commit form among them), README.md (what the module is, how an engine takes it
    as a submodule, `dotnet build` and `dotnet test`), `.github/STYLE.md`, `COMMITS.md` and
    `CODE_OF_CONDUCT.md` as the engines' with the engine's name replaced, `.gitignore` of the
    engines' shape with `/.github/dump/` in it, `.gitattributes`. The game's AGENTS.md and the
    engines' documents are the models; nothing is written that the module does not have yet. This
    commit is the first test of the commit form here, N 7.2 and N 7.6, read by the reviewing session
    before it is pushed. Done at `751c272`.

2. **The skeleton, one commit after item 1.** `Levels.slnx`, `Levels/Levels.csproj` as a class
    library on net10.0 with `Nullable` and `ImplicitUsings` in `Directory.Build.props`,
    `Levels.Tests/` on xunit with one test that the library loads, `build/version.txt` at `0.1` and
    `build/version.sh` as the engines', and `.github/workflows/build.yml` on a push, `test (os)` on
    ubuntu-24.04, windows-latest and macos-15 with `dotnet build Levels.slnx -warnaserror` and
    `dotnet test Levels.Tests --no-build`, a time limit on every job with its reason, `permissions`
    read and the engines' concurrency group. Green on the three systems before item 3. Done at
    `3ca2ac0`.

3. **The code's move.** The source is BevyCSharp's Levels, moved as it is, each engine binding the
    meshes and the collision to its own world; 3DEngine reads the format through this module for the
    first time. One commit a part, the tests coming with the code, the strict build and the suite
    green, a Shared line in the engines' REVIEW.md when a part lands. Items written by the reviewing
    session when item 2 is green and the owning session is free. The parts, proposed by BevyCSharp's
    session on 2026-10-31 at 14:10 and taken, one commit each with the tests beside the code, the
    namespace `Levels`. First the format and the geometry as they are, Level with its reading,
    writing and shapes, LevelEntity, LevelFace, LevelBrush and its shapes, LevelPoint, the two Vec3
    conversions becoming System.Numerics.Vector3, with LevelTests' parse, round trip, refusals,
    corners and meters and LevelShapesTests. Second the meshes and the collision as data without the
    edit mesh, so this module waits on nothing of Modeling's, Level.Meshes giving a LevelMesh a
    material with Vector3 positions and normals, Vector2 texture coordinates and uint indices in
    meters with Y up, the texel arithmetic and the PNG and JPEG size reader moving with it, the
    brushes giving a LevelHull a solid brush with its corners and planes in meters, convex, and a
    material's picture size coming through an interface the engine implements (N 2.1), with tests of
    the texels, the normals and the hull counts. Third TrenchBroom's GameConfig.cfg and a base fgd
    with worldspawn and info_player_start, an engine's own classes staying in its own fgd; the
    prototype pictures stay in BevyCSharp until Modeling's move, since the modeling paints them.

4. **The engines take the module.** Each engine adds this repository as a git submodule under
    `modules/` and references its project, its workflows checking submodules out, its own folder for
    this code becoming the binding of the module's interfaces (N 2.1) and the rest deleted; a change
    used by an engine is committed here first and pushed before the engine's commit that moves the
    pointer. One commit an engine, after item 3's first part. For BevyCSharp, its item 85, the
    binding keeps Levels.Load and Spawn renamed LevelSpawner (Decision 6), the entities, transforms,
    LevelKeys, the class registry, the materials by name, Render.CreateMesh from a LevelMesh and
    Bepu bodies from a LevelHull, a convex hull a brush in place of one mesh collider,
    WritePrototypePictures, the console commands, FeatureTestLevelTests and docs/levels.md pointing
    here.

## Verdicts

None yet.

## Decisions

1. **The repository's birth.** The owner made this repository on 2026-10-31 at 12:04 among six,
Modeling, Vegetation, Landscape, RadianceCascades, Tools beside it, after the ideas recorded as
3DEngine's Decision 78 and BevyCSharp's Decision 71, to hold the Valve 220 map format's reader, the
brushes to meshes and collision, and the TrenchBroom config and fgd as one library both engines use
rather than one implementation an engine. The owner set the order the same day, the documents first,
then the rest as the other projects have it, then the engines taking the module, and said the
modules may be tried at once beside the engine's global illumination work (3DEngine Decision 79).

2. **A git submodule, not a package.** The owner chose on 2026-10-31 that the engines take the
module as a git submodule built as a project of their own, since a package's upload, wait, test and
pack is slower to iterate than a commit. A change is committed here first; the reviewing session
pushes this repository before the engine's commit that moves its pointer; the workflows check
submodules out. The game keeps using the engine through the engine's package.

3. **The module knows no engine.** N 2.1. The module references no engine, package or bridge; what
it needs of a renderer, a world, a scheduler or a file system it takes through interfaces it
declares, which each engine implements where it binds the module. A type of an engine's in this
repository is a fault.

4. **The license is the owner's choice.** The repository was made with the Mozilla Public License
2.0, which stands; the owner may say otherwise.

5. **Who writes here.** The game's session writes items 1 and 2 for the six modules, being free
while the game waits on the engine's package; the session that owns the code today moves it (item
3), BevyCSharp's for Vegetation and Levels, 3DEngine's for Modeling and the cascades, the reviewing
session's scripts going into Tools; each engine's session binds the module (item 4). Every session
reads REVIEW.md's head, its items and the Replies before a batch, as N 7.7 says, and the reviewing
session pushes.

6. **The binding is named LevelSpawner.** With the format types in the namespace `Levels`,
    BevyCSharp's binding class `Bevy.Levels` would clash with it under a using, so it is renamed
    LevelSpawner, which says what it does, loads a level and spawns it; a public listing reshape as
    the owner's rule of no users until a release allows.

7. **BevyCSharp's session writes this module's commits.** It owns the code today, commits here with
    the owner's identity (N 7.6) and the form, and the reviewing session pushes, this repository
    before the engine's commit that moves the pointer.

## Replies
