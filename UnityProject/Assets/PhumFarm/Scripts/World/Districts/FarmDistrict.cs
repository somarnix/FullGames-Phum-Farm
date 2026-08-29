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
            WorldPrimitiveFactory.Box(root.transform, "Foundation", new Vector3(0, .18f, 0), new Vector3(size.x * 1.08f, .35f, size.z * 1.08f), WorldPrimitiveFactory.Hex("8B6A43"), false);
            WorldPrimitiveFactory.Box(root.transform, "Body", new Vector3(0, size.y * .32f, 0), new Vector3(size.x, size.y * .58f, size.z), WorldPrimitiveFactory.Hex(color), false);
            GameObject roofA = WorldPrimitiveFactory.Box(root.transform, "RoofLeft", new Vector3(-size.x * .22f, size.y * .72f, 0), new Vector3(size.x * .62f, .35f, size.z * 1.18f), WorldPrimitiveFactory.Hex("B64D32"), false);
            GameObject roofB = WorldPrimitiveFactory.Box(root.transform, "RoofRight", new Vector3(size.x * .22f, size.y * .72f, 0), new Vector3(size.x * .62f, .35f, size.z * 1.18f), WorldPrimitiveFactory.Hex("A8432D"), false);
            roofA.transform.localRotation = Quaternion.Euler(0, 0, -22f); roofB.transform.localRotation = Quaternion.Euler(0, 0, 22f);
            WorldPrimitiveFactory.Box(root.transform, "Door", new Vector3(0, size.y * .25f, -size.z * .505f), new Vector3(size.x * .22f, size.y * .42f, .12f), WorldPrimitiveFactory.Hex("5E3A25"), false);
            WorldPrimitiveFactory.Box(root.transform, "WindowLeft", new Vector3(-size.x * .3f, size.y * .38f, -size.z * .515f), new Vector3(size.x * .18f, size.y * .18f, .13f), WorldPrimitiveFactory.Hex("A9DCE3"), false);
            WorldPrimitiveFactory.Box(root.transform, "WindowRight", new Vector3(size.x * .3f, size.y * .38f, -size.z * .515f), new Vector3(size.x * .18f, size.y * .18f, .13f), WorldPrimitiveFactory.Hex("A9DCE3"), false);
        }

        protected static void FenceRect(Transform parent, Vector3 center, Vector2 size)
        {
            for (float x = center.x - size.x / 2; x <= center.x + size.x / 2; x += 2f)
            {
                WorldPrimitiveFactory.Box(parent, "BambooFence", new Vector3(x, .5f, center.z - size.y / 2), new Vector3(.14f, 1f, .14f), WorldPrimitiveFactory.Hex("A47C42"), false);
                WorldPrimitiveFactory.Box(parent, "BambooFence", new Vector3(x, .5f, center.z + size.y / 2), new Vector3(.14f, 1f, .14f), WorldPrimitiveFactory.Hex("A47C42"), false);
            }
            for (float z = center.z - size.y / 2; z <= center.z + size.y / 2; z += 2f)
            {
                WorldPrimitiveFactory.Box(parent, "BambooFence", new Vector3(center.x - size.x / 2, .5f, z), new Vector3(.14f, 1f, .14f), WorldPrimitiveFactory.Hex("A47C42"), false);
                WorldPrimitiveFactory.Box(parent, "BambooFence", new Vector3(center.x + size.x / 2, .5f, z), new Vector3(.14f, 1f, .14f), WorldPrimitiveFactory.Hex("A47C42"), false);
            }
            WorldPrimitiveFactory.Box(parent, "FenceRail", new Vector3(center.x, .62f, center.z-size.y/2), new Vector3(size.x,.12f,.12f), WorldPrimitiveFactory.Hex("B88B4C"), false);
            WorldPrimitiveFactory.Box(parent, "FenceRail", new Vector3(center.x, .62f, center.z+size.y/2), new Vector3(size.x,.12f,.12f), WorldPrimitiveFactory.Hex("B88B4C"), false);
        }
    }
}
