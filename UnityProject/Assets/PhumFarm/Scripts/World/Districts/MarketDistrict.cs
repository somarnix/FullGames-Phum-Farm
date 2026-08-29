using PhumFarm.Gameplay;
using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class MarketDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            WorldPrimitiveFactory.Box(transform, "MarketSquare", new Vector3(13, .05f, 13), new Vector3(21, .1f, 13), WorldPrimitiveFactory.Hex("CFB989"), false);
            Building(transform, "market_stall", "Village Market", new Vector3(13, 0, 15), new Vector3(9, 4, 5), "D8A34F");
            for (int i = 0; i < 4; i++)
            {
                string[] colors = { "D8583F", "E4B744", "6A9D57", "6D93B4" };
                WorldPrimitiveFactory.Box(transform, "ProduceBasket", new Vector3(8 + i * 3.1f, .45f, 10), new Vector3(2.3f, .8f, 1.8f), WorldPrimitiveFactory.Hex(colors[i]), false);
            }
            world.AddStation(transform, StationKind.Market, "market", "Village Market", new Vector3(13, 0, 11));
        }
    }
}
