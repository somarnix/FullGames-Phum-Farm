using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using PhumFarm.Core;

namespace PhumFarm
{
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        public Transform target;
        public float size = 23f;
        public Vector2 mapX = new(-38f, 38f);
        public Vector2 mapZ = new(-32f, 40f);
        private float yaw = 45f;
        private Camera cameraComponent;
        private Vector3 focusOffset;
        private Vector2 previousPointer;
        private float previousPinch;
        private bool dragging;
        private bool tutorialRecorded;

        private void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = size;
            cameraComponent.nearClipPlane = .1f;
            cameraComponent.farClipPlane = 180f;
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = World.WorldPrimitiveFactory.Hex("9ED6DD");
            focusOffset = new Vector3(8f, 0f, 8f);
            gameObject.tag = "MainCamera";
        }

        private void LateUpdate()
        {
            HandleRotation();
            HandleZoom();
            HandlePan();
            size = Mathf.Clamp(size, 12f, 42f);
            cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, size, Time.unscaledDeltaTime * 10f);
            if (target == null) return;
            Vector3 focus = target.position + focusOffset;
            focus.x = Mathf.Clamp(focus.x, mapX.x, mapX.y);
            focus.z = Mathf.Clamp(focus.z, mapZ.x, mapZ.y);
            focusOffset = focus - target.position;
            Quaternion rotation = Quaternion.Euler(48f, yaw, 0f);
            transform.position = Vector3.Lerp(transform.position, focus + rotation * new Vector3(0, 0, -35f), Time.unscaledDeltaTime * 7f);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.unscaledDeltaTime * 8f);
        }

        private void HandleRotation()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.qKey.wasPressedThisFrame) yaw -= 45f;
            if (keyboard.rKey.wasPressedThisFrame) yaw += 45f;
            if (keyboard.qKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame) RecordCameraTutorial();
        }

        private void HandleZoom()
        {
            if (Mouse.current != null)
            {
                float wheel = Mouse.current.scroll.ReadValue().y;
                size -= wheel * .012f;
                if (Mathf.Abs(wheel) > .1f) RecordCameraTutorial();
            }
            Touchscreen touch = Touchscreen.current;
            if (touch == null || !touch.touches[0].press.isPressed || !touch.touches[1].press.isPressed) { previousPinch = 0f; return; }
            float distance = Vector2.Distance(touch.touches[0].position.ReadValue(), touch.touches[1].position.ReadValue());
            if (previousPinch > 0f) size -= (distance - previousPinch) * .025f;
            if (previousPinch > 0f && Mathf.Abs(distance - previousPinch) > 2f) RecordCameraTutorial();
            previousPinch = distance;
        }

        private void HandlePan()
        {
            Vector2 pointer = default;
            bool held = false;
            if (Mouse.current != null)
            {
                held = Mouse.current.middleButton.isPressed || Mouse.current.rightButton.isPressed;
                pointer = Mouse.current.position.ReadValue();
            }
            Touchscreen touch = Touchscreen.current;
            if (touch != null && touch.touches[0].press.isPressed && !touch.touches[1].press.isPressed)
            {
                held = touch.touches[0].delta.ReadValue().sqrMagnitude > 4f;
                pointer = touch.touches[0].position.ReadValue();
            }
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) held = false;
            if (!held) { dragging = false; return; }
            if (!dragging) { dragging = true; previousPointer = pointer; return; }
            Vector2 delta = pointer - previousPointer;
            previousPointer = pointer;
            Vector3 right = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            Vector3 forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
            focusOffset -= (right * delta.x + forward * delta.y) * (size / Mathf.Max(1f, Screen.height)) * 1.8f;
            if (delta.sqrMagnitude > 4f) RecordCameraTutorial();
        }

        private void RecordCameraTutorial()
        {
            if (tutorialRecorded) return;
            tutorialRecorded = true;
            GameServices.Instance?.Tutorial.Record(TutorialAction.MoveCamera);
        }
    }
}
