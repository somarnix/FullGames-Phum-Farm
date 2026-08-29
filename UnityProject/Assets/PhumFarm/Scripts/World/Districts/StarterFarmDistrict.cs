using PhumFarm.Gameplay;
using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class StarterFarmDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            Building(transform, "farmhouse", "Khmer Farmhouse", new Vector3(-27, 0, -17), new Vector3(8, 6, 7), "D7B26D");
            Building(transform, "barn", "Rice Barn", new Vector3(-16, 0, -22), new Vector3(7, 5, 6), "C9884B");
            WorldPrimitiveFactory.Cylinder(transform, "WaterWell", new Vector3(-15, .55f, -13), new Vector3(1.3f, .55f, 1.3f), WorldPrimitiveFactory.Hex("8E785D"));
            WorldPrimitiveFactory.Cylinder(transform, "Pond", new Vector3(-29, .02f, -5), new Vector3(4.5f, .08f, 3.1f), WorldPrimitiveFactory.Hex("6BB5BE"), false);
            FenceRect(transform, new Vector3(-22, 0, -15), new Vector2(24, 21));
            world.AddStation(transform, StationKind.Home, "home", "Farmhouse", new Vector3(-27, 0, -17));
            world.AddStation(transform, StationKind.Well, "well", "Water Well", new Vector3(-15, 0, -13));
        }
    }
}
