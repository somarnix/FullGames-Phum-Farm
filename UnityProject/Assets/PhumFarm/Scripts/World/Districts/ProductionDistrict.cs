using PhumFarm.Gameplay;
using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class ProductionDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            Add(world, "feed_mill", "Feed Mill", "animal_feed", new Vector3(11, 0, -20), 1, "B98854");
            Add(world, "bakery", "Village Bakery", "bread", new Vector3(19, 0, -20), 3, "D6A25A");
            Add(world, "dairy", "Dairy Workshop", "butter", new Vector3(11, 0, -10), 5, "C9D4C4");
            Add(world, "sugar_mill", "Palm Sugar Mill", "palm_sugar", new Vector3(20, 0, -10), 7, "A77C4E");
        }

        private void Add(FarmWorldController world, string buildingId, string title, string productId, Vector3 position, int level, string color)
        {
            Building(transform, buildingId, title, position, new Vector3(6, 5, 5), color);
            world.AddStation(transform, StationKind.Production, productId, title, position + new Vector3(0, 0, -3.2f), level);
        }
    }
}
