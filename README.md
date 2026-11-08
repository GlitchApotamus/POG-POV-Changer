# POGPOVChanger

A Pit of Goblin mod that lets players toggle between first-person and third-person camera views.

## Default keybind

The mod defaults to the F5 key. If POG Config is installed, a custom POV toggle key can be assigned from the in-game config menu.

## Mod details

- Project: POG-POV-Changer
- Author: GlitchApotamus

## Third person

- Interact with items, levers and other objects (E by default) while in third person. A small aim dot is shown in the centre of the screen and turns green when something can be interacted with.
- Your held weapon swings while attacking, but your arms don't animate yet in third person.
- If the game forces you back to first person (for example when you are knocked down), interaction returns to normal.

## Requirements

- MelonLoader 0.7.3
- POG Config 1.0.10

## Building a release

This is an IL2CPP mod, so the DLL **must** be compiled against the game's generated
MelonLoader interop assemblies (`MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll` and
friends). Those are produced by MelonLoader from your installed copy of the game and are
specific to each game version, so they are **not** part of this repo.

Build a working release locally by pointing the build at your game folder:

```bash
dotnet build -c Release -p:GameDir="C:\Path\To\Steam\steamapps\common\Pit of Goblin"
# the packaged zip is written to bin/Release/net6.0/POGPOVChanger-<version>.zip
```

`-p:GameDir` is only needed if the game is not at the default Steam path. On Linux/Proton
pass the real path, e.g. `-p:GameDir="$HOME/.../steamapps/common/Pit of Goblin"`.

### Stub builds are a compile-check only

`BuildStubs/ModStubs.cs` lets the project compile without the game installed, but a build
made this way is **not shippable**: its `POGPOVChanger` class ends up subclassing a stub
`MelonLoader.MelonMod` that is baked into the DLL, so MelonLoader finds no real mod and never
loads it (no banner in the log, and the keybind does nothing). The build therefore refuses to
produce a mod package when it falls back to stubs, and prints a message telling you to supply
the game assemblies.

### CI releases

GitHub Actions runners don't have the game, so the release workflow builds a stub compile-check
by default and **does not publish** (there is nothing shippable to publish). To cut real
releases from CI, set a repository secret `GAME_ASSEMBLIES_URL` pointing at a private archive of
your game folder (containing `MelonLoader/net6/`, `MelonLoader/Il2CppAssemblies/` and
`Mods/POGConfig.dll`); the workflow restores it and builds against it. Otherwise, build and
publish releases locally with the command above.