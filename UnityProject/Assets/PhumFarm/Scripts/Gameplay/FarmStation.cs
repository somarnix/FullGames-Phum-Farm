using System;
using System.Collections.Generic;
using System.Linq;
using PhumFarm;
using PhumFarm.Core;
using PhumFarm.Data;
using UnityEngine;

namespace PhumFarm.Gameplay
{
    public enum StationKind { Production, Animal, Market, Expansion, Home, Well }

    public sealed class FarmStation : MonoBehaviour, IFarmInteractable
    {
        public event Action<FarmStation> ContextRequested;
        public StationKind Kind { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public int UnlockLevel { get; private set; }

        public string Prompt
        {
            get
            {
                GameServices services = GameServices.Instance;
                if (!services.Unlocks.Level(UnlockLevel)) return $"{Title} unlocks at level {UnlockLevel}";
                return Kind switch
                {
                    StationKind.Market => "Sell all farm goods",
                    StationKind.Production => ProductionPrompt(),
                    StationKind.Animal => AnimalPrompt(),
                    StationKind.Expansion => ExpansionPrompt(),
                    StationKind.Home => "Save and rest until morning",
                    StationKind.Well => "Water every growing crop",
                    _ => $"Use {Title}"
                };
            }
        }

        public void Setup(StationKind kind, string id, string title, int unlockLevel = 1)
        {
            Kind = kind; Id = id; Title = title; UnlockLevel = unlockLevel;
            name = id switch { "animal_feed" => "AnimalFeed", "chicken" => "Chickens", "cow" => "Cows", "market" => "Market", _ => id };
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(3f, 2.5f, 3f);
            collider.center = new Vector3(0, 1.25f, 0);
        }

        public void Interact(PlayerController player)
        {
            GameServices services = GameServices.Instance;
            if (!services.Unlocks.Level(UnlockLevel)) { services.State.NotifyMessage(Prompt); return; }
            if (Kind == StationKind.Market) SellGoods();
            else if (Kind == StationKind.Home) Rest();
            else if (Kind == StationKind.Well) WaterFields();
            else if (Kind == StationKind.Production && ReadyProduction()) PerformPrimaryAction();
            else ContextRequested?.Invoke(this);
        }

        public void PerformPrimaryAction()
        {
            switch (Kind)
            {
                case StationKind.Production: UseProduction(); break;
                case StationKind.Animal: UseAnimal(); break;
                case StationKind.Expansion: BuyExpansion(); break;
            }
        }

        private string ProductionPrompt()
        {
            GameServices services = GameServices.Instance;
            ProductDefinition product = services.Balance.Product(Id);
            ProductionJob job = services.State.Data.production.jobs.Find(value => value.productId == Id);
            if (job == null) return $"Make {Id}";
            if (services.State.GameMinutes >= job.readyAt) return $"Collect {Id}";
            return $"{Title} is working";
        }

        private bool ReadyProduction()
        {
            ProductionJob job = GameServices.Instance.State.Data.production.jobs.Find(value => value.productId == Id);
            return job != null && GameServices.Instance.State.GameMinutes >= job.readyAt;
        }

        private void UseProduction()
        {
            GameServices services = GameServices.Instance;
            ProductDefinition product = services.Balance.Product(Id);
            ProductionJob job = services.State.Data.production.jobs.Find(value => value.productId == Id);
            if (job != null)
            {
                if (services.State.GameMinutes < job.readyAt) { services.State.NotifyMessage(Prompt); return; }
                services.State.Data.production.jobs.Remove(job);
                services.Inventory.Add(Id, 1);
                services.Progression.AddXp(product.xp);
                services.State.NotifyMessage($"Collected {Id}");
                return;
            }
            var requirements = product.inputs.Select(value => new ItemStack { id = value.itemId, quantity = value.quantity }).ToList();
            if (!services.Inventory.Remove(requirements)) { services.State.NotifyMessage($"Missing ingredients for {Id}"); return; }
            services.State.Data.production.jobs.Add(new ProductionJob { productId = Id, startedAt = services.State.GameMinutes, readyAt = services.State.GameMinutes + product.seconds });
            services.State.NotifyMessage("Production started");
            services.State.NotifyChanged();
        }

        private string AnimalPrompt()
        {
            GameServices services = GameServices.Instance;
            AnimalJob job = services.State.Data.animals.jobs.Find(value => value.animalId == Id);
            if (job == null) return $"Prepare {Id} habitat";
            if (!job.fed) return $"Feed {Id}";
            return services.State.GameMinutes >= job.readyAt ? $"Collect from {Id}" : $"{Id} is producing";
        }

        private void UseAnimal()
        {
            GameServices services = GameServices.Instance;
            AnimalDefinition animal = services.Balance.Animal(Id);
            AnimalJob job = services.State.Data.animals.jobs.Find(value => value.animalId == Id);
            if (animal == null || job == null) { services.State.NotifyMessage($"{Title} is not configured yet"); return; }
            if (job.fed)
            {
                if (services.State.GameMinutes < job.readyAt) { services.State.NotifyMessage(Prompt); return; }
                job.fed = false;
                services.Inventory.Add(animal.productId, 2);
                services.Progression.AddXp(Id == "chicken" ? 10 : 16);
                services.State.NotifyMessage($"Collected {animal.productId}");
            }
            else
            {
                var feed = new List<ItemStack> { new() { id = "animal_feed", quantity = animal.feedQuantity } };
                if (!services.Inventory.Remove(feed)) { services.State.NotifyMessage("Make animal feed first"); return; }
                job.fed = true; job.readyAt = services.State.GameMinutes + animal.seconds;
                services.State.NotifyMessage($"{Id} fed");
            }
            services.State.NotifyChanged();
        }

        private void SellGoods()
        {
            GameServices services = GameServices.Instance;
            int total = 0;
            foreach (ItemStack item in services.State.Data.inventory.items)
            {
                if (item.quantity <= 0) continue;
                int price = services.Balance.Crop(item.id)?.sellPrice ?? services.Balance.Product(item.id)?.sellPrice ?? (item.id == "egg" ? 13 : item.id == "milk" ? 18 : 0);
                if (price <= 0) continue;
                total += price * item.quantity;
                item.quantity = 0;
            }
            if (total == 0) { services.State.NotifyMessage("No goods to sell yet"); return; }
            services.Economy.AddCoins(total);
            services.Progression.AddXp(Math.Max(1, total / 12));
            services.State.Data.player.tutorialStep = Math.Max(4, services.State.Data.player.tutorialStep);
            services.State.NotifyMessage($"Market sale: {total} coins");
            services.Saves.Save();
        }

        private string ExpansionPrompt()
        {
            GameServices services = GameServices.Instance;
            ExpansionDefinition expansion = services.Balance.Expansion(Id);
            if (services.Unlocks.Expansion(Id)) return $"{Id} is unlocked";
            return $"Unlock {Id} ({expansion.coinCost} coins, level {expansion.requiredLevel})";
        }

        private void BuyExpansion()
        {
            GameServices services = GameServices.Instance;
            ExpansionDefinition expansion = services.Balance.Expansion(Id);
            if (!services.Unlocks.CanBuyExpansion(Id)) { services.State.NotifyMessage(ExpansionPrompt()); return; }
            if (!services.Economy.SpendCoins(expansion.coinCost)) return;
            services.State.Data.world.expansions.Find(value => value.id == Id).unlocked = true;
            services.Progression.AddXp(35);
            services.State.NotifyMessage($"{Id} unlocked");
            services.Saves.Save();
            gameObject.SetActive(false);
        }

        private static void Rest()
        {
            GameServices services = GameServices.Instance;
            services.State.Data.world.day++;
            services.State.Data.world.minutes = 420f;
            services.Saves.Save();
            services.State.NotifyMessage("A new morning begins");
        }

        private static void WaterFields()
        {
            foreach (PlotState plot in GameServices.Instance.State.Data.crops.plots) if (plot.state == "growing") plot.plantedAt -= 8d;
            GameServices.Instance.State.NotifyChanged();
            GameServices.Instance.State.NotifyMessage("Fresh water helped every crop grow");
        }
    }
}
