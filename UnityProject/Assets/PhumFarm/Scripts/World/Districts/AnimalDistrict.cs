using PhumFarm.Gameplay;
using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class AnimalDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            FenceRect(transform, new Vector3(-25, 0, 17), new Vector2(15, 14));
            Building(transform, "chicken_coop", "Chicken Coop", new Vector3(-28, 0, 19), new Vector3(5, 4, 4), "D09B55");
            Building(transform, "cow_shelter", "Cow Shelter", new Vector3(-17, 0, 20), new Vector3(6, 4, 5), "B77947");
            for (int i = 0; i < 5; i++) Animal("chicken", "Chicken", new Vector3(-29 + i * 1.25f, .4f, 14 + (i % 2) * 1.4f), new Vector3(.65f, .75f, .65f), "F4E7BF");
            Animal("cow", "Cow", new Vector3(-18, .8f, 16), new Vector3(1.7f, 1.5f, 1f), "EDE5D5");
            Animal("buffalo", "Water Buffalo", new Vector3(-14, .85f, 20), new Vector3(1.8f, 1.55f, 1.1f), "67615B");
            world.AddStation(transform, StationKind.Animal, "chicken", "Chicken Coop", new Vector3(-27, 0, 16), 2);
            world.AddStation(transform, StationKind.Animal, "cow", "Cow Shelter", new Vector3(-17, 0, 17), 4);
        }

        private void Animal(string id, string title, Vector3 position, Vector3 scale, string color)
        {
            WorldPrimitiveFactory.ModelOrBox(transform, "Animals", id, title, position, scale, WorldPrimitiveFactory.Hex(color));
        }
    }
}
