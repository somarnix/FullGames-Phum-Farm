# Phum Farm — Unity

This is the Unity migration of Phum Farm. The original Godot project remains intact in the parent folder; no Godot scenes, scripts, data, or imported assets are removed by this Unity project.

## Open the project

1. Install or repair Unity `6000.5.10f1` in Unity Hub with the Windows Build Support module.
2. In Unity Hub, choose **Add > Add project from disk**.
3. Select the `UnityProject` folder, not the parent Godot folder.
4. Let Unity import packages and compile scripts.
5. Open `Assets/PhumFarm/Scenes/Boot.unity` and press Play. The scene is generated automatically on the first import. If needed, use **Phum Farm > Rebuild Boot Scene**.

On this computer, a verified clean editor is installed at `C:\Users\Sophanaroth Lem\UnityEditors\6000.5.10f1-clean`. You can also double-click `Launch-Phum-Farm-Unity.bat` in the parent project folder.

## Play controls

- Move: `WASD`, arrow keys, or click/tap a world position
- Interact: `E`, `Space`, or click an interactive farm object
- Camera: mouse wheel to zoom, `Q`/`R` to rotate
- The farm auto-saves when the app pauses or exits; the HUD also has a Save button.

## What has been ported

- Versioned save/load with temporary and backup files
- Game state, inventory, economy, XP progression, and unlock systems
- Persistent farm world split into reusable districts
- Tilling, planting, growing, harvesting, animal feeding, production, market selling, rest, watering, and land expansion
- Animated farm main menu plus the complete contextual HUD and bottom navigation
- Working shop, build mode, order delivery, barn inventory, district map, quest book, profile, settings, help, pause, station, expansion, and level-up screens
- English, Khmer, and Chinese localization with repaired Unicode source text
- Automatic Boot scene/build setup, five Edit Mode tests, and a full Boot/Play Mode UI parity test
- Resource-based model replacement paths for later FBX/GLB art

## Source preservation

Godot GDScript cannot be executed directly by Unity. The behavior has been translated to C#, while the original GDScript remains the migration reference. Unity save files use a separate filename (`phum_farm_unity_save.json`) so they cannot overwrite Godot saves.

See `MIGRATION.md` for the source mapping and known verification status.
