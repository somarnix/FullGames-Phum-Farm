using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class RiverOrchardDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            for (int row = 0; row < 3; row++)
            for (int column = 0; column < 3; column++)
            {
                Vector3 position = new Vector3(22 + column * 4f, 0, 19 + row * 4.5f);
                WorldPrimitiveFactory.Cylinder(transform, "BananaTrunk", position + Vector3.up * 1.5f, new Vector3(.25f, 1.5f, .25f), WorldPrimitiveFactory.Hex("80663A"));
                WorldPrimitiveFactory.Sphere(transform, "BananaLeaves", position + Vector3.up * 3.2f, new Vector3(2f, .45f, 2f), WorldPrimitiveFactory.Hex("5D9845"));
            }
            WorldPrimitiveFactory.Box(transform, "FishingDock", new Vector3(29, .3f, 13), new Vector3(9, .45f, 3), WorldPrimitiveFactory.Hex("8F613E"));
            WorldPrimitiveFactory.ModelOrBox(transform, "Props", "boat", "River Boat", new Vector3(34, .3f, 15), new Vector3(4, .7f, 1.6f), WorldPrimitiveFactory.Hex("A66A3F"));
        }
    }
}
