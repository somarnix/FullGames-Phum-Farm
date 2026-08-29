using UnityEngine;

namespace PhumFarm.World
{
    public static class WorldPrimitiveFactory
    {
        public static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool collider = true)
            => Primitive(PrimitiveType.Cube, parent, name, position, scale, color, collider);

        public static GameObject Sphere(Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool collider = false)
            => Primitive(PrimitiveType.Sphere, parent, name, position, scale, color, collider);

        public static GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool collider = false)
            => Primitive(PrimitiveType.Cylinder, parent, name, position, scale, color, collider);

        public static GameObject Primitive(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool collider)
        {
            GameObject value = GameObject.CreatePrimitive(type);
            value.name = name;
            value.transform.SetParent(parent, false);
            value.transform.localPosition = position;
            value.transform.localScale = scale;
            Renderer renderer = value.GetComponent<Renderer>();
            renderer.sharedMaterial = Material(color);
            Collider existing = value.GetComponent<Collider>();
            if (!collider && existing != null) Object.Destroy(existing);
            return value;
        }

        public static GameObject ModelOrBox(Transform parent, string category, string id, string name, Vector3 position, Vector3 fallbackScale, Color fallbackColor)
        {
            GameObject prefab = Resources.Load<GameObject>($"Models/{category}/{id}");
            if (prefab == null) return Box(parent, name, position, fallbackScale, fallbackColor, false);
            GameObject instance = Object.Instantiate(prefab, parent);
            instance.name = name;
            instance.transform.localPosition = position;
            return instance;
        }

        public static Color Hex(string value)
        {
            if (!value.StartsWith("#")) value = "#" + value;
            return ColorUtility.TryParseHtmlString(value, out Color color) ? color : Color.magenta;
        }

        private static Material Material(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            return material;
        }
    }
}
