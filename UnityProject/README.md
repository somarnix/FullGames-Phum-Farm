# Phum Farm Unity Project

This is the Phum Farm Unity project. Unity is the only supported engine and C# is the only gameplay language.

## Open the project

1. Install or repair Unity `6000.5.10f1` in Unity Hub with the Windows Build Support module.
2. In Unity Hub, choose **Add > Add project from disk**.
3. Select the `UnityProject` folder.
4. Let Unity import packages and compile scripts.
5. Open `Assets/PhumFarm/Scenes/Boot.unity` and press Play. The scene is generated automatically on the first import. If needed, use **Phum Farm > Rebuild Boot Scene**.

Double-click `Launch-Phum-Farm-Unity.bat` to open Boot and start the game automatically. Use `Launch-Phum-Farm-Unity-Editor.bat` when you want Boot open without entering Play Mode. Both launchers use Direct3D 11 because it renders the Unity editor reliably on this computer.

To repair the Unity Hub installation itself, double-click `Repair-Unity-Then-Launch.bat` and approve the Windows administrator prompt. It copies the matching Package Manager server from the verified editor, checks the executable hash, and opens Phum Farm with the repaired Hub editor.

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

## Source of truth

The Unity C# source, scenes, ScriptableObject definitions, tests, and Markdown knowledge base are the project source of truth. Save data uses `phum_farm_unity_save.json`.
