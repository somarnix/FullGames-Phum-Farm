# Phum Farm

Phum Farm is a playable Godot 4 isometric farming game set in a stylized Khmer-inspired countryside. It currently uses procedural low-poly placeholder art, so the complete game can run before custom FBX or GLB models are added.

The interface includes an animated farm main menu, compact HUD, crop and production panels, inventory, shop, orders, map, quest book, settings, pause, and level-up screens. The project is organized for future English, Khmer, and Chinese localization.

The commercial visual target is in `references/starter_farm_commercial_target.png`, with acceptance criteria in `references/ART_DIRECTION.md`.

## Play

Open `project.godot` in Godot 4.6 and press F5, or run:

```powershell
& 'C:\Users\Sophanaroth Lem\Desktop\Godot_v4.6-stable_win64.exe' --path .
```

Start a farm, walk to an open plot, till it, select Rice, plant, and harvest. Sell goods at the market. Progression unlocks plots, animal districts, workshops, and expansion gates.

## Controls

- WASD or arrow keys — move
- Left-click or tap the ground — walk there
- Left-click or tap an interactive object — walk there and use it
- E or Space — interact with the nearest object
- Q or R — rotate the isometric camera
- Mouse wheel or +/- — zoom
- Escape — pause

## Included gameplay

- 16 persistent plots with level-based unlocks
- Rice, corn, tomato, and sugarcane with visible growth stages
- Chickens, cows, and water buffalo with feeding and products
- Feed mill, bakery, dairy, and sugar workshop production timers
- Village market, coins, XP, ten levels, quests, and expansion purchases
- Day/night lighting, monsoon rain, home rest, and irrigation boost
- Inventory, contextual actions, settings, pause, and mobile-friendly UI
- JSON autosave and save/load migration support
- A connected world with crop fields, paddocks, workshops, market, village, orchard, ponds, canals, river, bridge, dock, and boat

## Adding FBX or GLB models

See `assets/models/README.md`. Copy a correctly named `.glb`, `.fbx`, or `.gltf` into the matching asset slot and let Godot import it. Gameplay logic, collision, scale normalization, UI, and saves remain separate from visual assets.

## Production structure

- `autoload/` — game, save, audio, scene, localization, inventory, economy, quest, order, notification, settings, and data managers
- `scenes/boot/` — splash, loading, and startup routing
- `scenes/menus/` — main menu, profile, language, settings, pause, credits, and exit confirmation
- `scenes/world/` — modular map sections 01–38 and the complete Phum Farm world
- `scenes/gameplay/` — crops, animals, buildings, production, orders, quests, expansion, and placement systems
- `scenes/characters/` — player and NPC foundations
- `scenes/camera/` — isometric camera, touch control, target, and bounds
- `scenes/ui/` — HUD, inventory, shop, dialogue, orders, quests, map, settings, tutorial, and shared components
- `assets/` — replaceable models, textures, audio, fonts, icons, and UI art
- `data/` — separate JSON balance data for crops, animals, buildings, recipes, levels, orders, quests, achievements, expansions, rewards, and economy
- `localization/` — English, Khmer, and Chinese translation sources and resources
- `resources/` and `shaders/` — themes, environments, settings, materials, and effects
- `references/` — art direction and visual reference
- `tests/` — structure, gameplay, and UI validation

The active entry point is `scenes/boot/boot.tscn`. The playable master world is `scenes/world/38_full_world/phum_farm_world.tscn` and the active interface is `scenes/menus/main_menu.tscn`.

## Verify

```powershell
& 'C:\Users\Sophanaroth Lem\Desktop\Godot_v4.6-stable_win64.exe' --headless --path . --script res://tests/test_project_structure.gd
& 'C:\Users\Sophanaroth Lem\Desktop\Godot_v4.6-stable_win64.exe' --headless --path . --script res://tests/smoke_test.gd
& 'C:\Users\Sophanaroth Lem\Desktop\Godot_v4.6-stable_win64.exe' --headless --path . --script res://tests/ui_test.gd
```

These checks validate the folder structure, all 38 world modules, gameplay systems, save/load, navigation, interface panels, player, and camera.
