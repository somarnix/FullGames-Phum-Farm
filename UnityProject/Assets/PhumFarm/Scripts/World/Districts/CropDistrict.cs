using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class CropDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            WorldPrimitiveFactory.Box(transform, "FieldBorder", new Vector3(-4, .06f, 9), new Vector3(20, .12f, 18), WorldPrimitiveFactory.Hex("647A41"), false);
            int index = 0;
            for (int row = 0; row < 4; row++)
            for (int column = 0; column < 4; column++)
            {
                world.AddPlot(transform, index++, new Vector3(-10 + column * 4.25f, 0, 3 + row * 3.7f));
            }
        }
    }
}
