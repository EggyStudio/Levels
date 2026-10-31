# Levels

Levels is a library both engines, 3DEngine and BevyCSharp, take as a git submodule, for the reader
of the Valve 220 map format, the meshes and collision shapes made from its brushes, and the
TrenchBroom configuration and FGD file those maps are edited with. It references no engine, and each
engine implements the interfaces it declares where it binds the module.

## Taking it into an engine

An engine adds the repository as a git submodule under `modules/` and references its project, here
from a project one folder below the engine's root.

```bash
git submodule add https://github.com/EggyStudio/Levels.git modules/Levels
```

```xml
<ProjectReference Include="..\modules\Levels\Levels\Levels.csproj" />
```

A clone of the engine fetches the module with `git clone --recurse-submodules`, or with
`git submodule update --init` in a clone made without it, and a workflow checks it out with
`submodules: true` on `actions/checkout`. A change the engine uses is committed and pushed here
before the engine's commit that moves its pointer, so the engine's history never names a commit
this repository does not have.

## Building and testing

With the .NET 10 SDK:

```bash
dotnet build Levels.slnx
dotnet test Levels.Tests
```

## License

The Mozilla Public License 2.0, in [LICENSE](LICENSE).
