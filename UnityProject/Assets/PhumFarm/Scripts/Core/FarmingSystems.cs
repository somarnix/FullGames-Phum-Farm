using System;
using System.Collections.Generic;
using System.Linq;
using PhumFarm.Data;

namespace PhumFarm.Core
{
    public sealed class CropSystem
    {
        private readonly GameState state; private readonly GameBalance balance; private readonly InventorySystem inventory;
        private readonly EconomySystem economy; private readonly ProgressionSystem progression; private readonly UnlockSystem unlocks; private readonly TutorialSystem tutorial;
        public CropSystem(GameState state, GameBalance balance, InventorySystem inventory, EconomySystem economy, ProgressionSystem progression, UnlockSystem unlocks, TutorialSystem tutorial)
        { this.state = state; this.balance = balance; this.inventory = inventory; this.economy = economy; this.progression = progression; this.unlocks = unlocks; this.tutorial = tutorial; }

        public bool Till(int index)
        {
            if (!Valid(index) || !unlocks.Plot(index)) return false;
            PlotState plot = state.Data.crops.plots[index];
            if (plot.state != "empty" && plot.state != "harvested") return false;
            plot.state = "tilled"; plot.cropId = string.Empty; plot.visualStage = -1; tutorial.Record(TutorialAction.TillField); state.NotifyChanged(); return true;
        }

        public bool Plant(int index, string cropId)
        {
            if (!Valid(index)) return false;
            PlotState plot = state.Data.crops.plots[index]; CropDefinition crop = balance.Crop(cropId);
            if (plot.state != "tilled" || crop == null || !unlocks.Crop(cropId) || !economy.SpendCoins(crop.seedCost)) return false;
            plot.state = "planted"; plot.cropId = cropId; plot.plantedAt = 0d; plot.visualStage = 0; state.Data.world.selectedCrop = cropId;
            if (cropId == "rice") tutorial.Record(TutorialAction.PlantRice); state.NotifyChanged(); return true;
        }

        public bool Water(int index)
        {
            if (!Valid(index)) return false;
            PlotState plot = state.Data.crops.plots[index]; if (plot.state != "planted") return false;
            plot.state = "sprout"; plot.plantedAt = state.GameMinutes; plot.visualStage = 1; state.NotifyChanged(); return true;
        }

        public string UpdateStage(int index)
        {
            PlotState plot = state.Data.crops.plots[index];
            if (plot.state != "sprout" && plot.state != "growing") return plot.state;
            float ratio = Growth(index); string next = ratio >= 1f ? "mature" : ratio >= .22f ? "growing" : "sprout";
            int stage = ratio >= 1f ? 4 : ratio >= .72f ? 3 : ratio >= .38f ? 2 : 1;
            if (next != plot.state || stage != plot.visualStage) { plot.state = next; plot.visualStage = stage; state.NotifyChanged(); }
            return plot.state;
        }

        public int Harvest(int index)
        {
            if (!Valid(index)) return 0;
            PlotState plot = state.Data.crops.plots[index]; UpdateStage(index); if (plot.state != "mature") return 0;
            CropDefinition crop = balance.Crop(plot.cropId); CropDefinitionAsset asset = balance.catalog?.Crop(plot.cropId);
            int amount = asset == null ? 3 : Math.Max(1, asset.harvestAmount); if (!inventory.Add(plot.cropId, amount, true)) return 0;
            progression.AddXp(crop.xp); state.Data.player.totalHarvested += amount; if (plot.cropId == "rice") tutorial.Record(TutorialAction.HarvestRice);
            GameServices.Instance?.Journey?.RecordHarvest(amount);
            plot.state = "harvested"; plot.visualStage = 5; state.NotifyChanged(); return amount;
        }

        public void ClearHarvested(int index)
        {
            PlotState plot = state.Data.crops.plots[index]; if (plot.state != "harvested") return;
            plot.state = "empty"; plot.cropId = string.Empty; plot.plantedAt = 0d; plot.visualStage = -1; state.NotifyChanged();
        }

        public float Growth(int index)
        {
            PlotState plot = state.Data.crops.plots[index]; CropDefinition crop = balance.Crop(plot.cropId);
            if (crop == null || plot.plantedAt <= 0d) return 0f;
            return Math.Max(0f, Math.Min(1f, (float)((state.GameMinutes - plot.plantedAt) / Math.Max(1f, crop.growSeconds))));
        }
        private bool Valid(int index) => index >= 0 && index < state.Data.crops.plots.Count;
    }

    public sealed class AnimalSystem
    {
        private readonly GameState state; private readonly GameBalance balance; private readonly InventorySystem inventory; private readonly ProgressionSystem progression; private readonly TutorialSystem tutorial;
        public AnimalSystem(GameState state, GameBalance balance, InventorySystem inventory, ProgressionSystem progression, TutorialSystem tutorial)
        { this.state = state; this.balance = balance; this.inventory = inventory; this.progression = progression; this.tutorial = tutorial; }

        public string Phase(string animalId)
        {
            AnimalJob job = Job(animalId); if (job == null) return "missing";
            if (!job.fed) return job.phase = "hungry";
            if (state.GameMinutes >= job.readyAt) return job.phase = "ready";
            double duration = Math.Max(1d, job.readyAt - job.startedAt);
            return job.phase = state.GameMinutes - job.startedAt < duration * .2d ? "eating" : "producing";
        }

        public bool Feed(string animalId)
        {
            AnimalDefinition animal = balance.Animal(animalId); AnimalJob job = Job(animalId);
            if (animal == null || job == null || state.Data.player.level < animal.unlockLevel || Phase(animalId) != "hungry") return false;
            AnimalDefinitionAsset asset = balance.catalog?.Animal(animalId); string item = asset == null ? "animal_feed" : asset.feedItem;
            if (!inventory.Remove(new[] { new ItemStack { id = item, quantity = animal.feedQuantity } })) return false;
            job.fed = true; job.phase = "eating"; job.startedAt = state.GameMinutes; job.readyAt = state.GameMinutes + animal.seconds;
            if (animalId == "chicken") tutorial.Record(TutorialAction.FeedChicken); state.NotifyChanged(); return true;
        }

        public int Collect(string animalId)
        {
            AnimalDefinition animal = balance.Animal(animalId); AnimalJob job = Job(animalId);
            if (animal == null || job == null || Phase(animalId) != "ready") return 0;
            AnimalDefinitionAsset asset = balance.catalog?.Animal(animalId); int amount = asset == null ? 2 : Math.Max(1, asset.productAmount);
            if (!inventory.Add(animal.productId, amount, true)) return 0;
            progression.AddXp(asset == null ? 10 : asset.xpReward); job.fed = false; job.phase = "hungry"; job.startedAt = 0d; job.readyAt = 0d;
            if (animal.productId == "egg") tutorial.Record(TutorialAction.CollectEgg); state.NotifyChanged(); return amount;
        }
        private AnimalJob Job(string id) => state.Data.animals.jobs.Find(value => value.animalId == id);
    }

    public sealed class ProductionSystem
    {
        private readonly GameState state; private readonly GameBalance balance; private readonly InventorySystem inventory; private readonly ProgressionSystem progression; private readonly EconomySystem economy; private readonly TutorialSystem tutorial;
        public ProductionSystem(GameState state, GameBalance balance, InventorySystem inventory, ProgressionSystem progression, EconomySystem economy, TutorialSystem tutorial)
        { this.state = state; this.balance = balance; this.inventory = inventory; this.progression = progression; this.economy = economy; this.tutorial = tutorial; }
        public IReadOnlyList<ProductionJob> Jobs(string productId) => state.Data.production.jobs.Where(value => value.productId == productId).OrderBy(value => value.readyAt).ToList();
        public bool Enqueue(string productId)
        {
            ProductDefinition product = balance.Product(productId); if (product == null || state.Data.player.level < product.unlockLevel || Jobs(productId).Count >= state.Data.production.queueSlots) return false;
            var requirements = product.inputs.Select(value => new ItemStack { id = value.itemId, quantity = value.quantity }).ToList();
            if (!inventory.Remove(requirements)) return false;
            double start = Math.Max(state.GameMinutes, Jobs(productId).Select(value => value.readyAt).DefaultIfEmpty(state.GameMinutes).Max());
            state.Data.production.jobs.Add(new ProductionJob { productId = productId, startedAt = start, readyAt = start + product.seconds });
            state.NotifyChanged(); return true;
        }
        public int Collect(string productId)
        {
            ProductDefinition product = balance.Product(productId); ProductionJob job = state.Data.production.jobs.Where(value => value.productId == productId && state.GameMinutes >= value.readyAt).OrderBy(value => value.readyAt).FirstOrDefault();
            if (product == null || job == null || !inventory.Add(productId, 1, true)) return 0;
            state.Data.production.jobs.Remove(job); GameServices.Instance?.Journey?.RecordProduct(); progression.AddXp(product.xp); state.NotifyChanged(); return 1;
        }
        public bool SpeedUp(string productId)
        {
            ProductionJob job = state.Data.production.jobs.Where(value => value.productId == productId && state.GameMinutes < value.readyAt).OrderBy(value => value.readyAt).FirstOrDefault();
            if (job == null || !economy.SpendGems(1)) return false;
            double saved = job.readyAt - state.GameMinutes;
            foreach (ProductionJob queued in Jobs(productId))
                if (queued != job && queued.startedAt >= job.readyAt) { queued.startedAt -= saved; queued.readyAt -= saved; }
            job.startedAt = Math.Min(job.startedAt, state.GameMinutes); job.readyAt = state.GameMinutes; state.NotifyChanged(); return true;
        }
    }
}
