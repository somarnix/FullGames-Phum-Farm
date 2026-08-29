using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class TerrainDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            WorldPrimitiveFactory.Box(transform, "WorldGround", new Vector3(0, -.2f, 0), new Vector3(92, .4f, 72), WorldPrimitiveFactory.Hex("7FA45A"));
            WorldPrimitiveFactory.Box(transform, "VillageRoad", new Vector3(-4, .03f, -6), new Vector3(82, .08f, 5), WorldPrimitiveFactory.Hex("C9AA74"), false);
            WorldPrimitiveFactory.Box(transform, "FarmPath", new Vector3(-10, .04f, -18), new Vector3(6, .09f, 28), WorldPrimitiveFactory.Hex("D0B47B"), false);
            WorldPrimitiveFactory.Box(transform, "River", new Vector3(31, .02f, 2), new Vector3(12, .12f, 72), WorldPrimitiveFactory.Hex("58A8B9"), false);
            WorldPrimitiveFactory.Box(transform, "Bridge", new Vector3(31, .35f, -7), new Vector3(13, .45f, 4), WorldPrimitiveFactory.Hex("9C6B3D"));

            for (int z = -32; z <= 32; z += 8)
            {
                Tree(new Vector3(-43, 0, z));
                Tree(new Vector3(43, 0, z));
            }
            for (int x = -36; x <= 36; x += 9)
            {
                Tree(new Vector3(x, 0, 33));
                Tree(new Vector3(x, 0, -33));
            }
        }

        private void Tree(Vector3 position)
        {
            var root = new GameObject("Palm"); root.transform.SetParent(transform, false); root.transform.localPosition = position;
            WorldPrimitiveFactory.Cylinder(root.transform, "Trunk", new Vector3(0, 2.2f, 0), new Vector3(.35f, 2.2f, .35f), WorldPrimitiveFactory.Hex("8F693E"));
            for (int index = 0; index < 5; index++)
            {
                GameObject leaf = WorldPrimitiveFactory.Sphere(root.transform, "Leaf", new Vector3(0, 4.55f, 0), new Vector3(2.4f, .25f, .7f), WorldPrimitiveFactory.Hex("4F8B4C"));
                leaf.transform.localRotation = Quaternion.Euler(0, index * 72f, index % 2 == 0 ? 12f : -12f);
            }
        }
    }
}
