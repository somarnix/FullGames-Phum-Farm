using UnityEngine;

namespace PhumFarm.World.Districts
{
    public abstract class FarmDistrict : MonoBehaviour
    {
        public abstract void Build(FarmWorldController world);

        protected static void Building(Transform parent, string id, string title, Vector3 position, Vector3 size, string color)
        {
            GameObject root = new(title); root.transform.SetParent(parent, false); root.transform.localPosition = position;
            GameObject model = Resources.Load<GameObject>($"Models/Buildings/{id}");
            if (model != null) { Object.Instantiate(model, root.transform); return; }
            WorldPrimitiveFactory.Box(root.transform, "Body", new Vector3(0, size.y * .3f, 0), new Vector3(size.x, size.y * .55f, size.z), WorldPrimitiveFactory.Hex(color), false);
            GameObject roof = WorldPrimitiveFactory.Box(root.transform, "Roof", new Vector3(0, size.y * .7f, 0), new Vector3(size.x * 1.12f, .38f, size.z * 1.15f), WorldPrimitiveFactory.Hex("B64D32"), false);
            roof.transform.rotation = Quaternion.Euler(0, 0, 8f);
        }

        protected static void FenceRect(Transform parent, Vector3 center, Vector2 size)
        {
            for (float x = center.x - size.x / 2; x <= center.x + size.x / 2; x += 2f)
            {
                WorldPrimitiveFactory.Box(parent, "BambooFence", new Vector3(x, .5f, center.z - size.y / 2), new Vector3(.14f, 1f, .14f), WorldPrimitiveFactory.Hex("A47C42"), false);
                WorldPrimitiveFactory.Box(parent, "BambooFence", new Vector3(x, .5f, center.z + size.y / 2), new Vector3(.14f, 1f, .14f), WorldPrimitiveFactory.Hex("A47C42"), false);
            }
        }
    }
}
