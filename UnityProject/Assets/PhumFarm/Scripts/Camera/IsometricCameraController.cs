using UnityEngine;

namespace PhumFarm
{
    [RequireComponent(typeof(Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        public Transform target;
        public float size = 25f;
        private float yaw = 45f;
        private Camera cameraComponent;

        private void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = size;
            gameObject.tag = "MainCamera";
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Q)) yaw -= 45f;
            if (Input.GetKeyDown(KeyCode.R)) yaw += 45f;
            size = Mathf.Clamp(size - Input.mouseScrollDelta.y * 2f, 16f, 42f);
            cameraComponent.orthographicSize = size;
            if (target == null) return;
            Quaternion rotation = Quaternion.Euler(48f, yaw, 0f);
            transform.position = Vector3.Lerp(transform.position, target.position + rotation * new Vector3(0, 0, -35f), Time.deltaTime * 5f);
            transform.rotation = rotation;
        }
    }
}
