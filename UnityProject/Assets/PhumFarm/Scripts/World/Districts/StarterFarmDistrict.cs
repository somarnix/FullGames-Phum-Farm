using PhumFarm.Gameplay;
using UnityEngine;

namespace PhumFarm.World.Districts
{
    public sealed class StarterFarmDistrict : FarmDistrict
    {
        public override void Build(FarmWorldController world)
        {
            Building(transform, "farmhouse", "Khmer Farmhouse", new Vector3(-27, 0, -17), new Vector3(8, 6, 7), "E2B96F");
            Building(transform, "barn", "Rice Barn", new Vector3(-16, 0, -22), new Vector3(7, 5, 6), "C86D45");
            BuildSilo(new Vector3(-8, 0, -22));
            BuildWindmill(new Vector3(10, 0, -18));
            BuildChickenPen(new Vector3(19, 0, -17));
            WorldPrimitiveFactory.Cylinder(transform, "WaterWell", new Vector3(-15, .55f, -13), new Vector3(1.3f, .55f, 1.3f), WorldPrimitiveFactory.Hex("8E785D"));
            WorldPrimitiveFactory.Cylinder(transform, "Pond", new Vector3(-29, .02f, -5), new Vector3(4.5f, .08f, 3.1f), WorldPrimitiveFactory.Hex("6BB5BE"), false);
            FenceRect(transform, new Vector3(-22, 0, -15), new Vector2(24, 21));
            world.AddStation(transform, StationKind.Home, "home", "Farmhouse", new Vector3(-27, 0, -17));
            world.AddStation(transform, StationKind.Well, "well", "Water Well", new Vector3(-15, 0, -13));
            world.AddStation(transform, StationKind.Production, "flour", "Windmill", new Vector3(10, 0, -21), 2);
            world.AddStation(transform, StationKind.Animal, "chicken", "Chicken Pen", new Vector3(19, 0, -20), 2);
        }

        private void BuildSilo(Vector3 p)
        {
            WorldPrimitiveFactory.Cylinder(transform,"Silo",p+Vector3.up*2.4f,new Vector3(2.1f,2.4f,2.1f),WorldPrimitiveFactory.Hex("D7C7A4"),false);
            WorldPrimitiveFactory.Sphere(transform,"SiloRoof",p+Vector3.up*4.75f,new Vector3(2.25f,.9f,2.25f),WorldPrimitiveFactory.Hex("B64D32"),false);
        }

        private void BuildWindmill(Vector3 p)
        {
            Building(transform,"windmill","Windmill",p,new Vector3(5,6,5),"E6C178");
            var hub=WorldPrimitiveFactory.Cylinder(transform,"WindmillHub",p+new Vector3(0,4.1f,-2.7f),new Vector3(.45f,.3f,.45f),WorldPrimitiveFactory.Hex("795238"),false); hub.transform.rotation=Quaternion.Euler(90,0,0);
            for(int i=0;i<4;i++){var blade=WorldPrimitiveFactory.Box(transform,"WindmillBlade",p+new Vector3(0,4.1f,-2.9f),new Vector3(.35f,4.5f,.16f),WorldPrimitiveFactory.Hex("F3E1B2"),false);blade.transform.rotation=Quaternion.Euler(0,0,i*90f);}
        }

        private void BuildChickenPen(Vector3 p)
        {
            FenceRect(transform,p,new Vector2(10,9));
            Building(transform,"chicken_coop","Chicken Pen",p+new Vector3(0,0,2),new Vector3(4,3.5f,3.5f),"D89B55");
            for(int i=0;i<4;i++){Vector3 q=p+new Vector3(-2.5f+i*1.6f,.45f,-1.5f+(i%2));WorldPrimitiveFactory.Sphere(transform,"Chicken",q,new Vector3(.7f,.8f,.7f),WorldPrimitiveFactory.Hex("FFF1CB"));WorldPrimitiveFactory.Sphere(transform,"ChickenHead",q+new Vector3(0,.55f,-.25f),Vector3.one*.4f,WorldPrimitiveFactory.Hex("FFF1CB"));}
        }
    }
}
