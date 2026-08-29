using System;
using PhumFarm.Core;
using PhumFarm.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PhumFarm
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public event Action<string> PromptChanged;
        public float speed = 6f;

        private CharacterController controller;
        private Vector3 moveTarget;
        private bool hasMoveTarget;
        private IFarmInteractable pendingInteraction;
        private bool movementTutorialRecorded;

        private void Awake() => controller = GetComponent<CharacterController>();

        public void BuildVisual()
        {
            GameObject model = Resources.Load<GameObject>("Models/Characters/farmer");
            if (model != null) { Instantiate(model, transform); return; }
            World.WorldPrimitiveFactory.Cylinder(transform, "Body", new Vector3(0, 1f, 0), new Vector3(.7f, .8f, .7f), World.WorldPrimitiveFactory.Hex("4D8F67"));
            World.WorldPrimitiveFactory.Sphere(transform, "Head", new Vector3(0, 1.75f, 0), Vector3.one * .62f, World.WorldPrimitiveFactory.Hex("B97850"));
            World.WorldPrimitiveFactory.Cylinder(transform, "Hat", new Vector3(0, 2.05f, 0), new Vector3(.9f, .08f, .9f), World.WorldPrimitiveFactory.Hex("D7A53F"));
        }

        private void Update()
        {
            Vector2 move = ReadMove();
            Vector3 input = new(move.x, 0f, move.y);
            if (input.sqrMagnitude > .02f) { hasMoveTarget = false; pendingInteraction = null; }
            else if (hasMoveTarget)
            {
                Vector3 delta = moveTarget - transform.position;
                delta.y = 0f;
                if (delta.magnitude < .28f) hasMoveTarget = false;
                else input = delta.normalized;
            }
            if (input.sqrMagnitude > 1f) input.Normalize();
            controller.SimpleMove(input * speed);
            if (input.sqrMagnitude > .02f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(input), Time.deltaTime * 10f);
                if (!movementTutorialRecorded) movementTutorialRecorded = true;
            }

            IFarmInteractable nearest = FindNearest();
            PromptChanged?.Invoke(nearest?.Prompt ?? string.Empty);
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.eKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) nearest?.Interact(this);
            if (pendingInteraction != null && Vector3.Distance(transform.position, ((MonoBehaviour)pendingInteraction).transform.position) < 3.4f)
            {
                pendingInteraction.Interact(this);
                pendingInteraction = null;
            }
            HandlePointer();
        }

        private void HandlePointer()
        {
            bool pressed = false;
            Vector2 pointer = default;
            int pointerId = -1;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) { pressed = true; pointer = Mouse.current.position.ReadValue(); }
            Touchscreen touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            {
                pressed = true; pointer = touch.primaryTouch.position.ReadValue(); pointerId = touch.primaryTouch.touchId.ReadValue();
            }
            if (!pressed || EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId)) return;
            Camera camera = Camera.main;
            if (camera == null || !Physics.Raycast(camera.ScreenPointToRay(pointer), out RaycastHit hit, 250f)) return;
            IFarmInteractable target = FindInteractable(hit.collider.transform);
            if (target != null)
            {
                if (Vector3.Distance(transform.position, hit.collider.transform.position) < 3.5f) target.Interact(this);
                else { pendingInteraction = target; WalkTo(hit.collider.transform.position); }
            }
            else WalkTo(hit.point);
        }

        private static Vector2 ReadMove()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;
            float x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            return new Vector2(x, y);
        }

        public void WalkTo(Vector3 point)
        {
            moveTarget = new Vector3(point.x, transform.position.y, point.z);
            hasMoveTarget = true;
            if (!movementTutorialRecorded) movementTutorialRecorded = true;
        }

        public void InteractNearest() => FindNearest()?.Interact(this);

        private IFarmInteractable FindNearest()
        {
            IFarmInteractable best = null;
            float distance = 3.5f;
            foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude))
            {
                if (behaviour is not IFarmInteractable candidate || !behaviour.isActiveAndEnabled) continue;
                float candidateDistance = Vector3.Distance(transform.position, behaviour.transform.position);
                if (candidateDistance >= distance) continue;
                distance = candidateDistance;
                best = candidate;
            }
            return best;
        }

        private static IFarmInteractable FindInteractable(Transform source)
        {
            while (source != null)
            {
                foreach (MonoBehaviour behaviour in source.GetComponents<MonoBehaviour>()) if (behaviour is IFarmInteractable value) return value;
                source = source.parent;
            }
            return null;
        }
    }
}
