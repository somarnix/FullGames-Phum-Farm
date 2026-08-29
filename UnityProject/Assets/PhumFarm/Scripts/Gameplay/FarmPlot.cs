using System;
using System.Collections;
using PhumFarm.Core;
using PhumFarm.Data;
using PhumFarm.World;
using UnityEngine;

namespace PhumFarm.Gameplay
{
    public sealed class FarmPlot : MonoBehaviour, IFarmInteractable
    {
        public event Action<FarmPlot> CropSelectionRequested;
        public int Index { get; private set; }
        private Renderer soil;
        private Transform cropRoot;
        private string visualSignature = string.Empty;

        public string Prompt
        {
            get
            {
                GameServices services = GameServices.Instance;
                if (!services.Unlocks.Plot(Index)) return $"Plot unlocks at level {services.Balance.plotUnlockLevels[Index]}";
                PlotState plot = services.State.Data.crops.plots[Index];
                if (plot.state == "empty" || plot.state == "harvested") return "Till this plot";
                if (plot.state == "tilled") return "Choose a crop to plant";
                if (plot.state == "planted") return $"Water {plot.cropId}";
                CropDefinition crop = services.Balance.Crop(plot.cropId);
                return plot.state == "mature" ? $"Harvest {plot.cropId}" : $"{plot.cropId} is {plot.state} - {(int)(Growth(plot, crop) * 100f)}%";
            }
        }

        public void Setup(int index)
        {
            Index = index;
            name = $"FarmPlot_{index + 1:00}";
            GameObject baseObject = WorldPrimitiveFactory.Box(transform, "Soil", new Vector3(0, .14f, 0), new Vector3(3.25f, .28f, 2.6f), WorldPrimitiveFactory.Hex("A36B45"));
            soil = baseObject.GetComponent<Renderer>();
            Color ridge = WorldPrimitiveFactory.Hex("70452D");
            for (int row = -1; row <= 1; row++)
                WorldPrimitiveFactory.Box(transform, "SoilFurrow", new Vector3(0, .31f, row * .72f), new Vector3(2.85f, .09f, .16f), ridge, false);
            WorldPrimitiveFactory.Box(transform, "PlotEdge", new Vector3(0, .3f, -1.22f), new Vector3(3.35f, .16f, .13f), ridge, false);
            WorldPrimitiveFactory.Box(transform, "PlotEdge", new Vector3(0, .3f, 1.22f), new Vector3(3.35f, .16f, .13f), ridge, false);
            cropRoot = new GameObject("Crops").transform;
            cropRoot.SetParent(transform, false);
            Refresh(true);
        }

        public void Interact(PlayerController player)
        {
            GameServices services = GameServices.Instance;
            if (!services.Unlocks.Plot(Index)) { services.State.NotifyMessage(Prompt); return; }
            PlotState plot = services.State.Data.crops.plots[Index];
            services.Tutorial.Record(TutorialAction.SelectField);
            if (plot.state == "empty" || plot.state == "harvested")
            {
                services.Crops.Till(Index);
                services.State.NotifyMessage("Soil tilled. Choose a seed.");
            }
            else if (plot.state == "tilled")
            {
                CropSelectionRequested?.Invoke(this);
                return;
            }
            else if (plot.state == "planted")
            {
                Water();
                return;
            }
            else if (plot.state == "mature")
            {
                string cropId = plot.cropId;
                int harvestAmount = services.Crops.Harvest(Index);
                if (harvestAmount <= 0)
                {
                    services.State.NotifyMessage("Barn is full");
                    return;
                }
                services.State.NotifyMessage($"Harvested {harvestAmount} {cropId}");
                StartCoroutine(HarvestFeedback(cropId));
                Invoke(nameof(ClearHarvested), .35f);
            }
            else services.State.NotifyMessage(Prompt);
            services.State.NotifyChanged();
            Refresh(true);
        }

        public bool Plant(string cropId)
        {
            GameServices services = GameServices.Instance;
            CropDefinition crop = services.Balance.Crop(cropId);
            if (crop == null || !services.Crops.Plant(Index, cropId)) return false;
            services.State.NotifyMessage($"{cropId} planted. Water it to start growth.");
            services.State.NotifyChanged();
            Refresh(true);
            return true;
        }

        public bool Water()
        {
            PlotState plot = GameServices.Instance.State.Data.crops.plots[Index];
            if (!GameServices.Instance.Crops.Water(Index)) return false;
            GameServices.Instance.State.NotifyMessage($"{plot.cropId} watered");
            GameServices.Instance.State.NotifyChanged();
            Refresh(true);
            return true;
        }

        private void ClearHarvested()
        {
            GameServices.Instance.Crops.ClearHarvested(Index);
            Refresh(true);
        }

        private void Update()
        {
            if (Time.frameCount % 15 != Index % 15) return;
            UpdateGrowthState();
            Refresh(false);
        }

        private void UpdateGrowthState()
        {
            GameServices.Instance.Crops.UpdateStage(Index);
        }

        private void Refresh(bool force)
        {
            if (soil == null || cropRoot == null || GameServices.Instance == null) return;
            GameServices services = GameServices.Instance;
            PlotState plot = services.State.Data.crops.plots[Index];
            string signature = $"{services.Unlocks.Plot(Index)}:{plot.state}:{plot.cropId}:{plot.visualStage}";
            if (!force && signature == visualSignature) return;
            visualSignature = signature;
            foreach (Transform child in cropRoot) Destroy(child.gameObject);
            if (!services.Unlocks.Plot(Index)) { soil.material.color = WorldPrimitiveFactory.Hex("66795B"); return; }
            soil.material.color = WorldPrimitiveFactory.Hex(plot.state == "empty" || plot.state == "harvested" ? "A36B45" : "75543B");
            if (string.IsNullOrEmpty(plot.cropId)) return;
            CropDefinition crop = services.Balance.Crop(plot.cropId);
            CropDefinitionAsset asset = services.Balance.catalog?.Crop(plot.cropId);
            int stage = Mathf.Clamp(plot.visualStage, 0, 4);
            if (asset != null && stage < asset.growthStagePrefabs.Length && asset.growthStagePrefabs[stage] != null)
            {
                Instantiate(asset.growthStagePrefabs[stage], cropRoot);
                return;
            }
            float scale = stage switch { 0 => .12f, 1 => .28f, 2 => .52f, 3 => .78f, _ => 1f };
            Color color = WorldPrimitiveFactory.Hex(crop.color);
            if (crop.id == "wheat") BuildWheat(stage, scale);
            else for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
                WorldPrimitiveFactory.Cylinder(cropRoot, crop.id, new Vector3(x * .8f, .22f + scale * .45f, z * .65f), new Vector3(.12f + scale * .07f, .15f + scale * .55f, .12f + scale * .07f), color);
        }

        private void BuildWheat(int stage, float scale)
        {
            Color stem = WorldPrimitiveFactory.Hex(stage >= 4 ? "E9B93F" : stage >= 3 ? "B8B849" : "67A84B");
            Color head = WorldPrimitiveFactory.Hex("F2C852");
            for (int x = 0; x < 4; x++) for (int z = 0; z < 4; z++)
            {
                float px = -1.05f + x * .7f + (z % 2) * .08f;
                float pz = -.9f + z * .6f;
                float height = .25f + scale * 1.25f + ((x + z) % 3) * .05f;
                WorldPrimitiveFactory.Cylinder(cropRoot, "WheatStem", new Vector3(px, .28f + height * .5f, pz), new Vector3(.045f, height * .5f, .045f), stem);
                if (stage >= 3)
                    WorldPrimitiveFactory.Sphere(cropRoot, "WheatHead", new Vector3(px, .3f + height, pz), new Vector3(.13f, .24f, .13f), head);
            }
        }

        private IEnumerator HarvestFeedback(string cropId)
        {
            Color color = cropId == "wheat" ? WorldPrimitiveFactory.Hex("FFD45C") : WorldPrimitiveFactory.Hex("FFF0A0");
            var pieces = new GameObject[8];
            for (int i = 0; i < pieces.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / pieces.Length;
                pieces[i] = WorldPrimitiveFactory.Sphere(transform, "HarvestSpark", new Vector3(Mathf.Cos(angle) * .45f, .7f, Mathf.Sin(angle) * .45f), Vector3.one * .18f, color);
            }
            float elapsed = 0f;
            while (elapsed < .55f)
            {
                elapsed += Time.deltaTime;
                for (int i = 0; i < pieces.Length; i++) if (pieces[i] != null)
                    pieces[i].transform.localPosition += new Vector3(Mathf.Cos(i * Mathf.PI / 4f), 1.8f, Mathf.Sin(i * Mathf.PI / 4f)) * Time.deltaTime;
                yield return null;
            }
            foreach (GameObject piece in pieces) if (piece != null) Destroy(piece);
        }

        private static float Growth(PlotState plot, CropDefinition crop) => crop == null || plot.plantedAt <= 0d ? 0f : Mathf.Clamp01((float)((GameServices.Instance.State.GameMinutes - plot.plantedAt) / Math.Max(1f, crop.growSeconds)));
    }
}
