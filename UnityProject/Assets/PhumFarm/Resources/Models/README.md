# Replacing generated art

Phum Farm runs without imported models. The Unity runtime creates colorful placeholder geometry whenever a model prefab is absent.

To replace a placeholder:

1. Import an FBX or GLB into the matching folder below.
2. Configure its materials, scale, rig, and colliders in Unity.
3. Drag the imported model into the Project window to create a prefab.
4. Give the prefab the exact ID used by the game.

Expected prefab locations include:

- `Buildings/farmhouse`, `Buildings/barn`, `Buildings/chicken_coop`
- `Animals/chicken`, `Animals/cow`, `Animals/buffalo`
- `Characters/farmer`
- `Props/boat`

Keep gameplay scripts on the generated parent object. Art prefabs are visual children, so replacing art does not remove interactions or saved data.
