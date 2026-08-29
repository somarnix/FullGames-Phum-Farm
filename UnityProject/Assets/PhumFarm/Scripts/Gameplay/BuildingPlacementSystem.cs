using System;
using System.Collections.Generic;
using System.Linq;
using PhumFarm.Core;
using PhumFarm.Data;
using PhumFarm.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PhumFarm.Gameplay
{
    public sealed class BuildingPlacementSystem : MonoBehaviour
    {
        public event Action<bool> ValidityChanged;
        public bool IsActive => preview != null;
        public bool IsValid { get; private set; }
        public string SelectedBuildingId => definition == null ? string.Empty : definition.id;

        private readonly Dictionary<string, GameObject> instances = new();
        private Transform placedRoot;
        private GameObject preview;
        private BuildingDefinitionAsset definition;
        private BuildingPlacement moving;
        private float rotationY;
        private Vector3 lastPosition;

        public void Initialize()
        {
            placedRoot = new GameObject("PlayerBuildings").transform;
            placedRoot.SetParent(transform, false);
            RestoreAll();
        }

        public bool BeginPlace(string buildingId)
        {
            Cancel();
            definition = GameServices.Instance.Balance.catalog?.Building(buildingId);
            if (definition == null || !GameServices.Instance.Unlocks.Building(buildingId)) return false;
            rotationY = 0f;
            CreatePreview(Vector3.zero);
            return true;
        }

        public bool BeginMove(string instanceId)
        {
            BuildingPlacement placement = GameServices.Instance.State.Data.buildings.placements.Find(value => value.instanceId == instanceId && !value.stored);
            if (placement == null) return false;
            Cancel();
            definition = GameServices.Instance.Balance.catalog?.Building(placement.buildingId);
            if (definition == null) return false;
            moving = placement;
            rotationY = placement.rotationY;
            if (instances.TryGetValue(instanceId, out GameObject existing)) existing.SetActive(false);
            CreatePreview(placement.position);
            return true;
        }

        public void Rotate()
        {
            if (!IsActive) return;
            rotationY = (rotationY + 90f) % 360f;
            preview.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            Validate();
        }

        public bool Confirm()
        {
            if (!IsActive || !IsValid) { GameServices.Instance.State.NotifyMessage("Choose a clear unlocked grid tile"); return false; }
            if (moving == null && !GameServices.Instance.Economy.SpendCoins(definition.cost)) return false;
            BuildingPlacement placement = moving ?? new BuildingPlacement { instanceId = Guid.NewGuid().ToString("N"), buildingId = definition.id };
            placement.position = preview.transform.position;
            placement.rotationY = rotationY;
            placement.stored = false;
            if (moving == null) GameServices.Instance.State.Data.buildings.placements.Add(placement);
            if (instances.TryGetValue(placement.instanceId, out GameObject old)) Destroy(old);
            instances[placement.instanceId] = CreatePermanent(placement);
            Destroy(preview);
            preview = null; definition = null; moving = null;
            GameServices.Instance.State.NotifyChanged();
            GameServices.Instance.Saves.Save();
            return true;
        }

        public bool Store()
        {
            if (moving == null) return false;
            moving.stored = true;
            if (instances.TryGetValue(moving.instanceId, out GameObject existing)) { Destroy(existing); instances.Remove(moving.instanceId); }
            Destroy(preview);
            preview = null; definition = null; moving = null;
            GameServices.Instance.State.NotifyChanged();
            GameServices.Instance.Saves.Save();
            return true;
        }

        public void Cancel()
        {
            if (moving != null && instances.TryGetValue(moving.instanceId, out GameObject existing)) existing.SetActive(true);
            if (preview != null) Destroy(preview);
            preview = null; definition = null; moving = null;
        }

        public void ClearAll()
        {
            Cancel();
            foreach (GameObject value in instances.Values) if (value != null) Destroy(value);
            instances.Clear();
            GameServices.Instance.State.Data.buildings.placements.Clear();
            GameServices.Instance.State.NotifyChanged();
        }

        private void Update()
        {
            if (!IsActive) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.rKey.wasPressedThisFrame) Rotate();
                if (keyboard.escapeKey.wasPressedThisFrame) Cancel();
                if (keyboard.enterKey.wasPressedThisFrame) Confirm();
            }
            if (!TryPointer(out Vector2 screen, out bool released) || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Camera camera = Camera.main;
            if (camera == null || !Physics.Raycast(camera.ScreenPointToRay(screen), out RaycastHit hit, 250f)) return;
            SetPreviewPosition(hit.point);
            // Placement is intentionally confirmed only through the Build UI or Enter key.
        }

        public void SetPreviewPosition(Vector3 position)
        {
            if (!IsActive) return;
            lastPosition = new Vector3(Mathf.Round(position.x), 0f, Mathf.Round(position.z));
            preview.transform.position = lastPosition;
            Validate();
        }

        private void Validate()
        {
            if (!IsActive) return;
            bool valid = IsInsideUnlockedLand(lastPosition) && !Overlaps(lastPosition);
            if (valid != IsValid) { IsValid = valid; ValidityChanged?.Invoke(valid); }
            Color color = valid ? new Color(.25f, .95f, .35f, .65f) : new Color(.95f, .2f, .18f, .65f);
            foreach (Renderer renderer in preview.GetComponentsInChildren<Renderer>()) renderer.material.color = color;
        }

        private bool IsInsideUnlockedLand(Vector3 position)
        {
            Bounds starter = new(Vector3.zero, new Vector3(42f, 4f, 42f));
            if (starter.Contains(position + Vector3.up)) return true;
            foreach (ExpansionDefinitionAsset expansion in GameServices.Instance.Balance.catalog?.expansions ?? new List<ExpansionDefinitionAsset>())
                if (expansion != null && GameServices.Instance.Unlocks.Expansion(expansion.id) && expansion.buildBounds.Contains(position + Vector3.up)) return true;
            return false;
        }

        private bool Overlaps(Vector3 position)
        {
            Vector3 size = new(definition.size.x * .48f, 1.4f, definition.size.y * .48f);
            foreach (Collider collider in Physics.OverlapBox(position + Vector3.up * 1.4f, size, Quaternion.Euler(0f, rotationY, 0f)))
            {
                if (collider.transform.IsChildOf(preview.transform)) continue;
                if (moving != null && instances.TryGetValue(moving.instanceId, out GameObject current) && collider.transform.IsChildOf(current.transform)) continue;
                if (collider.bounds.max.y <= .35f || collider.name.Contains("Ground") || collider.name.Contains("Terrain")) continue;
                return true;
            }
            return false;
        }

        private void CreatePreview(Vector3 position)
        {
            preview = CreateVisual("PlacementGhost", position, rotationY, false);
            SetPreviewPosition(position);
        }

        private GameObject CreatePermanent(BuildingPlacement placement)
        {
            definition = GameServices.Instance.Balance.catalog?.Building(placement.buildingId);
            GameObject value = CreateVisual(placement.instanceId, placement.position, placement.rotationY, true);
            definition = null;
            return value;
        }

        private GameObject CreateVisual(string objectName, Vector3 position, float rotation, bool collider)
        {
            GameObject root = new(objectName);
            root.transform.SetParent(placedRoot, false);
            root.transform.position = position;
            root.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            if (definition.prefab != null)
            {
                Instantiate(definition.prefab, root.transform);
                if (collider && root.GetComponentInChildren<Collider>() == null) root.AddComponent<BoxCollider>().size = new Vector3(definition.size.x, 2.5f, definition.size.y);
            }
            else
            {
                WorldPrimitiveFactory.Box(root.transform, "Body", new Vector3(0, 1.1f, 0), new Vector3(definition.size.x, 2.2f, definition.size.y), definition.fallbackColor, collider);
                WorldPrimitiveFactory.Box(root.transform, "Roof", new Vector3(0, 2.35f, 0), new Vector3(definition.size.x * 1.12f, .35f, definition.size.y * 1.12f), WorldPrimitiveFactory.Hex("B64D32"), false);
            }
            return root;
        }

        private void RestoreAll()
        {
            foreach (BuildingPlacement placement in GameServices.Instance.State.Data.buildings.placements.Where(value => !value.stored))
                if (!string.IsNullOrEmpty(placement.instanceId)) instances[placement.instanceId] = CreatePermanent(placement);
        }

        private static bool TryPointer(out Vector2 position, out bool released)
        {
            position = default; released = false;
            if (Mouse.current != null && (Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasReleasedThisFrame))
            {
                position = Mouse.current.position.ReadValue(); released = Mouse.current.leftButton.wasReleasedThisFrame; return true;
            }
            Touchscreen touch = Touchscreen.current;
            if (touch != null && (touch.primaryTouch.press.isPressed || touch.primaryTouch.press.wasReleasedThisFrame))
            {
                position = touch.primaryTouch.position.ReadValue(); released = touch.primaryTouch.press.wasReleasedThisFrame; return true;
            }
            return false;
        }
    }
}
