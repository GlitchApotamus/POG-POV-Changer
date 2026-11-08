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
- POG Config 1.0.13

## Building a release

This is an IL2CPP mod, so the DLL must be compiled against the game's generated
MelonLoader interop assemblies (`MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll` and
friends). Those are produced by MelonLoader from your installed copy of the game and are
specific to each game version, so they are not part of this repo.

Build a working release locally by pointing the build at your game folder:

```bash
dotnet build -c Release \
  -p:GameDir="C:\Path\To\Steam\steamapps\common\Pit of Goblin" \
  -p:MelonLoaderDir="C:\Path\To\Steam\steamapps\common\Pit of Goblin\MelonLoader" \
  -p:ModsDir="C:\Path\To\Steam\steamapps\common\Pit of Goblin\Mods"
# the packaged zip is written to bin/Release/net6.0/POGPOVChanger-<version>.zip
```

If you are building on Linux/Proton, pass the real paths instead:

```bash
dotnet build -c Release \
  -p:GameDir="$HOME/.steam/steam/steamapps/common/Pit of Goblin" \
  -p:MelonLoaderDir="$HOME/.steam/steam/steamapps/common/Pit of Goblin/MelonLoader" \
  -p:ModsDir="$HOME/.steam/steam/steamapps/common/Pit of Goblin/Mods"
```

`-p:GameDir` is only needed if the game is not at the default Steam path. On Linux/Proton,
pass the real path, e.g. `-p:GameDir="$HOME/.../steamapps/common/Pit of Goblin"`.

This project is meant to be built locally and published by hand. The repo no longer includes
stub-build support, so there is no fake compile-check path anymore; if the real assemblies are
not present, the build fails instead of shipping a broken mod.
