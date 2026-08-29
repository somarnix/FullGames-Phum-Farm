# Drop-in 3D model slots

This folder is the stable replacement interface for custom FBX/GLB art. Add a model with one of the names below and restart Godot. The game automatically prefers `.glb`, then `.fbx`, then `.gltf`; when no file exists it keeps the playable procedural placeholder.

Godot imports models asynchronously after they are copied here. Wait for the import spinner to finish before running the game.

## Buildings

Place in `buildings/`:

- `farmhouse.glb`
- `barn.glb`
- `chicken_coop.glb`
- `cow_shelter.glb`
- `feed_mill.glb`
- `bakery.glb`
- `dairy.glb`
- `sugar_mill.glb`
- `village_house.glb`

## Animals

Place in `animals/`:

- `chicken.glb`
- `cow.glb`
- `water_buffalo.glb`

## Crops

Place in `crops/`. Every crop supports four growth stages:

- `rice_stage_1.glb` through `rice_stage_4.glb`
- `corn_stage_1.glb` through `corn_stage_4.glb`
- `tomato_stage_1.glb` through `tomato_stage_4.glb`
- `sugarcane_stage_1.glb` through `sugarcane_stage_4.glb`

## Character and props

- `characters/farmer.glb`
- `props/sugar_palm.glb`

## Export contract

- Godot units are metres. Model at real scale; the loader also normalizes to the intended height.
- Put the origin at ground level and centered horizontally.
- Use `+Y` up and face `+Z`.
- Apply transforms before export.
- Prefer one atlas material per mobile model and embedded or adjacent textures.
- Building target: 3,000–15,000 triangles; animal: 1,000–6,000; crop plant: 100–800.
- For the farmer, animation clips can be named `Idle` and `Walk` (case-insensitive fallbacks are supported).
- Do not add signs or text to models. All visible language remains in Godot UI.

Collision stays supplied by the game for buildings and interaction stations, so replacement meshes do not need collision nodes.
