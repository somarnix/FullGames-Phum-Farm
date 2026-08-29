using System;
using PhumFarm;
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

        public string Prompt
        {
            get
            {
                GameServices services = GameServices.Instance;
                if (!services.Unlocks.Plot(Index)) return $"Plot unlocks at level {services.Balance.plotUnlockLevels[Index]}";
                PlotState plot = services.State.Data.crops.plots[Index];
                if (plot.state == "empty") return "Till this plot";
                if (plot.state == "tilled") return "Choose a crop to plant";
                CropDefinition crop = services.Balance.Crop(plot.cropId);
                return IsReady(plot, crop) ? $"Harvest {plot.cropId}" : $"{plot.cropId} is growing - {(int)(Growth(plot, crop) * 100f)}%";
            }
        }

        public void Setup(int index)
        {
            Index = index;
            name = $"FarmPlot_{index + 1:00}";
            GameObject baseObject = WorldPrimitiveFactory.Box(transform, "Soil", new Vector3(0, .14f, 0), new Vector3(3.25f, .28f, 2.6f), WorldPrimitiveFactory.Hex("A36B45"));
            soil = baseObject.GetComponent<Renderer>();
            cropRoot = new GameObject("Crops").transform;
            cropRoot.SetParent(transform, false);
            Refresh();
        }

        public void Interact(PlayerController player)
        {
            GameServices services = GameServices.Instance;
            if (!services.Unlocks.Plot(Index)) { services.State.NotifyMessage(Prompt); return; }
            PlotState plot = services.State.Data.crops.plots[Index];
            if (plot.state == "empty")
            {
                plot.state = "tilled";
                services.State.Data.player.tutorialStep = Math.Max(1, services.State.Data.player.tutorialStep);
                services.State.NotifyMessage("Soil tilled. Choose a seed.");
            }
            else if (plot.state == "tilled") { CropSelectionRequested?.Invoke(this); return; }
            else
            {
                CropDefinition crop = services.Balance.Crop(plot.cropId);
                if (!IsReady(plot, crop)) { services.State.NotifyMessage(Prompt); return; }
                services.Inventory.Add(plot.cropId, 3);
                services.Progression.AddXp(crop.xp);
                services.State.Data.player.totalHarvested += 3;
                services.State.Data.player.tutorialStep = Math.Max(3, services.State.Data.player.tutorialStep);
                services.State.NotifyMessage($"Harvested 3 {plot.cropId}");
                plot.state = "empty"; plot.cropId = string.Empty; plot.plantedAt = 0;
            }
            services.State.NotifyChanged();
            Refresh();
        }

        public bool Plant(string cropId)
        {
            GameServices services = GameServices.Instance;
            PlotState plot = services.State.Data.crops.plots[Index];
            CropDefinition crop = services.Balance.Crop(cropId);
            if (plot.state != "tilled" || crop == null || !services.Unlocks.Crop(cropId) || !services.Economy.SpendCoins(crop.seedCost)) return false;
            plot.state = "growing"; plot.cropId = cropId; plot.plantedAt = services.State.GameMinutes;
            services.State.Data.world.selectedCrop = cropId;
            services.State.Data.player.tutorialStep = Math.Max(2, services.State.Data.player.tutorialStep);
            services.State.NotifyMessage($"{cropId} planted");
            services.State.NotifyChanged();
            Refresh();
            return true;
        }

        private void Update() { if (Time.frameCount % 30 == Index % 30) Refresh(); }

        private void Refresh()
        {
            if (soil == null || GameServices.Instance == null) return;
            foreach (Transform child in cropRoot) Destroy(child.gameObject);
            GameServices services = GameServices.Instance;
            if (!services.Unlocks.Plot(Index)) { soil.sharedMaterial.color = WorldPrimitiveFactory.Hex("66795B"); return; }
            PlotState plot = services.State.Data.crops.plots[Index];
            soil.sharedMaterial.color = WorldPrimitiveFactory.Hex(plot.state == "empty" ? "A36B45" : "75543B");
            if (plot.state != "growing") return;
            CropDefinition crop = services.Balance.Crop(plot.cropId);
            float ratio = Growth(plot, crop);
            Color color = WorldPrimitiveFactory.Hex(crop.color);
            for (int x = -1; x <= 1; x++) for (int z = -1; z <= 1; z++)
                WorldPrimitiveFactory.Cylinder(cropRoot, crop.id, new Vector3(x * .8f, .25f + ratio * .5f, z * .65f), new Vector3(.12f, .2f + ratio * .6f, .12f), color);
        }

        private static float Growth(PlotState plot, CropDefinition crop) => Mathf.Clamp01((float)((GameServices.Instance.State.GameMinutes - plot.plantedAt) / Math.Max(1f, crop.growSeconds)));
        private static bool IsReady(PlotState plot, CropDefinition crop) => plot.state == "growing" && Growth(plot, crop) >= 1f;
    }
}
