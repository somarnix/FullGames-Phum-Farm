# Unity Game Architecture

## Project layers

- App: boot, loading, scene flow, service lifetime.
- Core: state, save, time, events, inventory, economy, progression.
- Data: JSON balance, ScriptableObject definitions, localization.
- Gameplay: crops, animals, production, orders, quests, placement.
- World: farm controller, districts, expansion gates, weather.
- Presentation: camera, character, UI, audio, animation, effects.
- Editor: catalog builder, validation, scene creation, build tools.
- Tests: Edit Mode rules and Play Mode player journeys.

## Data ownership

Runtime systems read definitions and modify only the save state they own. UI reads state and calls public gameplay operations. UI does not edit save fields directly. World objects display state; they do not become the only source of truth.

## Content rule

The crop engine knows `CropDefinition`, not rice. The production engine knows `RecipeDefinition`, not bread. This allows content growth without copying code.

## Save contract

Use an explicit save version. Write to a temporary file, read it back, preserve a backup, and replace the main file only after verification. Add migration code whenever persisted fields change.

## Scene contract

Boot creates persistent services and routes to Main Menu. Main Menu starts or continues a farm. Farm World contains the playable world and UI. A clean checkout must rebuild required scenes through the editor bootstrap.

## Dependency direction

Data does not depend on UI. Core does not depend on scene objects. Gameplay may depend on Core and Data. Presentation may depend on public gameplay interfaces. Editor code stays in an Editor assembly.
