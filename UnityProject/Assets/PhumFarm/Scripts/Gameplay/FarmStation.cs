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
            List<ProductionJob> jobs = services.State.Data.production.jobs.FindAll(value => value.productId == Id);
            if (jobs.Exists(value => services.Time.IsReady(value.readyAt))) return $"Collect {Id}";
            if (jobs.Count < services.State.Data.production.queueSlots) return $"Make {Id} ({jobs.Count}/{services.State.Data.production.queueSlots})";
            return $"{Title} queue is full";
        }

        private bool ReadyProduction()
        {
            return GameServices.Instance.State.Data.production.jobs.Exists(value => value.productId == Id && GameServices.Instance.Time.IsReady(value.readyAt));
        }

        private void UseProduction()
        {
            GameServices services = GameServices.Instance;
            ProductDefinition product = services.Balance.Product(Id);
            if (product == null) return;
            if (services.Production.Jobs(Id).Any(value => services.Time.IsReady(value.readyAt)))
            {
                if (services.Production.Collect(Id) > 0) services.State.NotifyMessage($"Collected {Id}");
                else services.State.NotifyMessage("Barn is full");
                return;
            }
            if (!services.Production.Enqueue(Id)) { services.State.NotifyMessage(services.Production.Jobs(Id).Count >= services.State.Data.production.queueSlots ? "Production queue is full" : $"Missing ingredients for {Id}"); return; }
            services.State.NotifyMessage("Production started");
        }

        private string AnimalPrompt()
        {
            GameServices services = GameServices.Instance;
            AnimalJob job = services.State.Data.animals.jobs.Find(value => value.animalId == Id);
            if (job == null) return $"Prepare {Id} habitat";
            string phase = services.Animals.Phase(Id);
            return phase switch
            {
                "hungry" => $"Feed {Id}",
                "ready" => $"Collect from {Id}",
                "eating" => $"{Id} is eating",
                _ => $"{Id} is producing"
            };
        }

        private void UseAnimal()
        {
            GameServices services = GameServices.Instance;
            AnimalDefinition animal = services.Balance.Animal(Id);
            AnimalJob job = services.State.Data.animals.jobs.Find(value => value.animalId == Id);
            if (animal == null || job == null) { services.State.NotifyMessage($"{Title} is not configured yet"); return; }
            string phase = services.Animals.Phase(Id);
            if (phase == "ready")
            {
                int amount = services.Animals.Collect(Id);
                services.State.NotifyMessage(amount > 0 ? $"Collected {amount} {animal.productId}" : "Barn is full");
            }
            else if (phase == "hungry")
            {
                services.State.NotifyMessage(services.Animals.Feed(Id) ? $"{Id} fed" : "Make animal feed first");
            }
            else services.State.NotifyMessage(Prompt);
            services.State.NotifyChanged();
        }

        public bool SpeedUpProduction()
        {
            if (Kind != StationKind.Production) return false;
            return GameServices.Instance.Production.SpeedUp(Id);
        }

        private void SellGoods()
        {
            GameServices services = GameServices.Instance;
            int total = 0;
            foreach (ItemStack item in services.State.Data.inventory.items)
            {
                if (item.quantity <= 0) continue;
                int price = services.Balance.catalog?.Item(item.id)?.sellPrice ?? services.Balance.Crop(item.id)?.sellPrice ?? services.Balance.Product(item.id)?.sellPrice ?? 0;
                if (price <= 0) continue;
                total += price * item.quantity;
                item.quantity = 0;
            }
            if (total == 0) { services.State.NotifyMessage("No goods to sell yet"); return; }
            services.Economy.AddCoins(total);
            services.Progression.AddXp(Math.Max(1, total / 12));
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
            FindAnyObjectByType<PhumFarm.World.FarmWorldController>()?.RefreshExpansions();
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
            foreach (PlotState plot in GameServices.Instance.State.Data.crops.plots)
            {
                if (plot.state == "planted") { plot.state = "sprout"; plot.plantedAt = GameServices.Instance.State.GameMinutes; plot.visualStage = 1; }
                else if (plot.state == "sprout" || plot.state == "growing") plot.plantedAt -= 8d;
            }
            GameServices.Instance.State.NotifyChanged();
            GameServices.Instance.State.NotifyMessage("Fresh water helped every crop grow");
        }
    }
}
