using System;
using System.Collections.Generic;
using PhumFarm;
using PhumFarm.Core;
using PhumFarm.Gameplay;
using PhumFarm.World.Districts;
using UnityEngine;

namespace PhumFarm.World
{
    public sealed class FarmWorldController : MonoBehaviour
    {
        public event Action<FarmPlot> CropSelectionRequested;
        public event Action<FarmStation> StationContextRequested;
        public PlayerController Player { get; private set; }
        public Camera Camera { get; private set; }
        public List<FarmPlot> Plots { get; } = new();
        public List<FarmStation> Stations { get; } = new();

        public static FarmWorldController Create()
        {
            var root = new GameObject("FarmWorld");
            var world = root.AddComponent<FarmWorldController>();
            world.Build();
            return world;
        }

        private void Update() => GameServices.Instance.State.AdvanceTime(Time.deltaTime);

        private void Build()
        {
            var environment = new GameObject("Environment"); environment.transform.SetParent(transform);
            BuildEnvironment(environment.transform);
            var districts = new GameObject("Districts"); districts.transform.SetParent(transform);
            AddDistrict<TerrainDistrict>(districts.transform);
            AddDistrict<StarterFarmDistrict>(districts.transform);
            AddDistrict<CropDistrict>(districts.transform);
            AddDistrict<AnimalDistrict>(districts.transform);
            AddDistrict<ProductionDistrict>(districts.transform);
            AddDistrict<MarketDistrict>(districts.transform);
            AddDistrict<RiverOrchardDistrict>(districts.transform);
            AddDistrict<ExpansionDistrict>(districts.transform);
            BuildPlayerAndCamera();
        }

        private void AddDistrict<T>(Transform parent) where T : FarmDistrict
        {
            var node = new GameObject(typeof(T).Name.Replace("District", "District"));
            node.transform.SetParent(parent, false);
            T district = node.AddComponent<T>();
            district.Build(this);
        }

        public FarmPlot AddPlot(Transform parent, int index, Vector3 position)
        {
            var node = new GameObject(); node.transform.SetParent(parent, false); node.transform.localPosition = position;
            FarmPlot plot = node.AddComponent<FarmPlot>();
            plot.Setup(index);
            plot.CropSelectionRequested += value => CropSelectionRequested?.Invoke(value);
            Plots.Add(plot);
            return plot;
        }

        public FarmStation AddStation(Transform parent, StationKind kind, string id, string title, Vector3 position, int level = 1)
        {
            var node = new GameObject(); node.transform.SetParent(parent, false); node.transform.localPosition = position;
            FarmStation station = node.AddComponent<FarmStation>();
            station.Setup(kind, id, title, level);
            station.ContextRequested += value => StationContextRequested?.Invoke(value);
            Stations.Add(station);
            return station;
        }

        public FarmStation Station(string name) => Stations.Find(value => value.name == name);

        private void BuildPlayerAndCamera()
        {
            var characters = new GameObject("Characters"); characters.transform.SetParent(transform);
            var playerObject = new GameObject("Player"); playerObject.transform.SetParent(characters.transform); playerObject.transform.position = new Vector3(-18, .2f, -13);
            var controller = playerObject.AddComponent<CharacterController>(); controller.height = 1.8f; controller.radius = .38f; controller.center = new Vector3(0, .9f, 0);
            Player = playerObject.AddComponent<PlayerController>(); Player.BuildVisual();
            var cameraObject = new GameObject("IsometricCamera"); cameraObject.transform.SetParent(transform);
            cameraObject.tag = "MainCamera";
            Camera = cameraObject.AddComponent<Camera>();
            Camera.clearFlags = CameraClearFlags.SolidColor;
            Camera.backgroundColor = WorldPrimitiveFactory.Hex("728C83");
            var cameraController = cameraObject.AddComponent<IsometricCameraController>(); cameraController.target = Player.transform;
        }

        private static void BuildEnvironment(Transform parent)
        {
            RenderSettings.ambientLight = WorldPrimitiveFactory.Hex("D8E0C2");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = WorldPrimitiveFactory.Hex("A9C8B6");
            RenderSettings.fogStartDistance = 55f;
            RenderSettings.fogEndDistance = 105f;
            var sunObject = new GameObject("Sun"); sunObject.transform.SetParent(parent); sunObject.transform.rotation = Quaternion.Euler(52f, -35f, 0f);
            Light sun = sunObject.AddComponent<Light>(); sun.type = LightType.Directional; sun.intensity = 1.35f; sun.color = WorldPrimitiveFactory.Hex("FFF1CC"); sun.shadows = LightShadows.Soft;
        }
    }
}
