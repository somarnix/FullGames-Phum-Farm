# Godot-to-Unity migration map

The Unity project is intentionally isolated under `UnityProject/`. It does not convert or overwrite the Godot project in place.

| Godot concept | Unity implementation |
| --- | --- |
| Autoload singletons | `GameServices` persistent component |
| `GameState` dictionaries | Typed serializable C# save models |
| `SaveManager` | Atomic JSON save plus backup in `Application.persistentDataPath` |
| `.tscn` scenes | Runtime-built components and an auto-generated Boot scene |
| `FarmWorld` districts | `FarmWorldController` plus district components |
| `farm_plot.gd` | `FarmPlot.cs` |
| `farm_station.gd` | `FarmStation.cs` |
| Godot UI controls | Unity uGUI created by `GameUiController` |
| `res://assets/models` | `Assets/PhumFarm/Resources/Models` prefabs |

## Why the code is translated

Unity does not run GDScript or Godot `.tscn` files. The original logic is preserved in the repository and translated into equivalent C# architecture. Game data and balance values are retained in JSON.

## Verification status

- Godot source: parser, project structure, smoke, and UI tests passed before migration work continued.
- Unity source: structure and static compatibility checks can run immediately.
- Unity compilation, Edit Mode tests, Play Mode, and build validation require the Unity Editor installation to be completed in Hub.

## Models later

Import FBX directly. For GLB, use Unity's supported importer/package workflow, turn the imported object into a prefab, then place it at the exact `Resources/Models/<Category>/<id>` path documented in `Assets/PhumFarm/Resources/Models/README.md`.
