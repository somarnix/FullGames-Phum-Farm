using System;
using System.Collections.Generic;
using System.Linq;
using PhumFarm.Core;
using PhumFarm.Data;
using PhumFarm.Gameplay;
using PhumFarm.World;
using UnityEngine;
using UnityEngine.UI;

namespace PhumFarm.UI
{
    public sealed partial class GameUiController
    {
        private sealed class FarmOrder
        {
            public string Id;
            public string Title;
            public int Coins;
            public int Xp;
            public ItemStack[] Items;
        }

        private static readonly FarmOrder[] FarmOrders =
        {
            new() { Id = "village_breakfast", Title = "Village Breakfast", Coins = 64, Xp = 18, Items = new[] { Stack("rice", 3), Stack("egg", 1) } },
            new() { Id = "market_basket", Title = "Market Basket", Coins = 92, Xp = 24, Items = new[] { Stack("corn", 2), Stack("tomato", 2) } },
            new() { Id = "bakery_delivery", Title = "Bakery Delivery", Coins = 115, Xp = 30, Items = new[] { Stack("bread", 1), Stack("milk", 1) } }
        };

        private void ShowInventory()
        {
            GameServices services = GameServices.Instance;
            services.Tutorial.Record(TutorialAction.OpenInventory);
            GameObject card = OpenModal("BARN INVENTORY", 1120, 720);
            Label(card.transform, "Capacity", $"Barn capacity  {services.Inventory.UsedCapacity} / {services.Inventory.Capacity}", 21, FontStyle.Bold,
                TextAnchor.MiddleLeft, new Vector2(0, 1), Vector2.one, new Vector2(48, -132), new Vector2(-48, -88), cream);
            MakeProgressBar(card.transform, "CapacityBar", (float)services.Inventory.UsedCapacity / services.Inventory.Capacity,
                new Vector2(0, 1), Vector2.one, new Vector2(48, -157), new Vector2(-48, -137));

            string[] items = { "rice", "corn", "tomato", "sugarcane", "egg", "milk", "animal_feed", "bread", "butter", "palm_sugar" };
            for (int i = 0; i < items.Length; i++)
            {
                int row = i / 5;
                int column = i % 5;
                string id = items[i];
                float left = 48 + column * 205;
                float top = -190 - row * 202;
                GameObject itemCard = Panel(card.transform, id, new Color(.17f, .23f, .17f, 1f), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, top - 178), new Vector2(left + 185, top));
                Label(itemCard.transform, "Icon", ItemGlyph(id), 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .46f), Vector2.one,
                    new Vector2(8, 3), new Vector2(-8, -8), gold);
                Label(itemCard.transform, "Name", TitleCase(id), 19, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1, .45f),
                    new Vector2(8, 36), new Vector2(-8, -2), cream);
                Label(itemCard.transform, "Amount", $"x {services.Inventory.Amount(id)}", 24, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero,
                    new Vector2(1, .28f), new Vector2(8, 0), new Vector2(-8, 36), Color.white);
            }

            Label(card.transform, "Hint", "Harvest crops, collect animal goods, and finish production to fill your barn.", 19, FontStyle.Italic,
                TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1, 0), new Vector2(30, 20), new Vector2(-30, 72), new Color(1, 1, 1, .72f));
        }

        private void ShowShop()
        {
            GameServices services = GameServices.Instance;
            GameObject card = OpenModal("VILLAGE SHOP", 1160, 760);
            AddTabStrip(card.transform, new[] { "FEATURED", "FARM", "ANIMALS", "PRODUCTION", "DECOR" }, 0);
            BuildingDefinitionAsset[] offers = services.Balance.catalog.buildings.Where(value => value != null && value.cost > 0).Take(8).ToArray();
            for (int i = 0; i < offers.Length; i++)
            {
                BuildingDefinitionAsset offer = offers[i];
                int row = i / 4;
                int column = i % 4;
                float left = 42 + column * 275;
                float top = -180 - row * 228;
                bool unlocked = services.Unlocks.Building(offer.id);
                GameObject offerCard = Panel(card.transform, offer.id, new Color(.18f, .235f, .17f, 1f), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, top - 204), new Vector2(left + 255, top));
                Label(offerCard.transform, "Art", BuildingGlyph(offer.id), 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .54f), Vector2.one,
                    new Vector2(8, 2), new Vector2(-8, -6), gold);
                Label(offerCard.transform, "Name", TitleCase(offer.id), 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .32f), new Vector2(1, .55f),
                    new Vector2(8, 0), new Vector2(-8, 0), cream);
                string priceText = unlocked ? $"{offer.cost} COINS" : $"UNLOCKS LEVEL {offer.unlockLevel}";
                Button buy = MakeButton(offerCard.transform, "Select", priceText, Vector2.zero, new Vector2(1, .30f), new Vector2(12, 12), new Vector2(-12, -4),
                    unlocked ? green : WorldPrimitiveFactory.Hex("515650"), () => SelectShopOffer(offer.id));
                buy.interactable = unlocked;
            }
        }

        private void SelectShopOffer(string id)
        {
            if (world?.Placement == null || !world.Placement.BeginPlace(id)) { ShowToast("That building is not available yet"); return; }
            CloseModal();
            ShowToast($"{TitleCase(id)} selected. Tap a clear tile, then open Build to confirm.");
        }

        private void ShowBuildMode()
        {
            GameObject card = OpenModal("BUILD MODE", 1080, 680);
            string selected = world?.Placement != null && world.Placement.IsActive ? TitleCase(world.Placement.SelectedBuildingId) : "No building selected - open Shop or choose a placed building below";
            Label(card.transform, "Help", selected + "\nDrag/tap the ground to position the ghost. Green is valid; red is blocked.",
                23, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one, new Vector2(60, -190), new Vector2(-60, -95), cream);
            string[] labels = { "MOVE", "ROTATE", "STORE", "CONFIRM", "CANCEL" };
            for (int i = 0; i < labels.Length; i++)
            {
                int captured = i;
                float left = 38 + i * 205;
                MakeButton(card.transform, labels[i], labels[i], new Vector2(0, 1), new Vector2(0, 1), new Vector2(left, -285), new Vector2(left + 185, -215),
                    i == 3 ? green : i == 4 ? terracotta : WorldPrimitiveFactory.Hex("75533A"), () =>
                    {
                        if (world?.Placement == null) return;
                        if (captured == 0) { CloseModal(); ShowToast("Drag the ghost on the farm, then reopen Build to confirm"); }
                        else if (captured == 1) { world.Placement.Rotate(); ShowToast("Building rotated 90 degrees"); }
                        else if (captured == 2) { if (world.Placement.Store()) ShowToast("Building returned to storage"); CloseModal(); }
                        else if (captured == 3) { if (world.Placement.Confirm()) { ShowToast("Building placed and farm saved"); CloseModal(); } }
                        else { world.Placement.Cancel(); CloseModal(); }
                    });
            }
            Label(card.transform, "PlacedTitle", "PLACED BUILDINGS - SELECT TO MOVE", 19, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one,
                new Vector2(40, -350), new Vector2(-40, -305), gold);
            List<BuildingPlacement> placed = GameServices.Instance.State.Data.buildings.placements.Where(value => !value.stored).Take(6).ToList();
            for (int i = 0; i < placed.Count; i++)
            {
                BuildingPlacement capturedPlacement = placed[i];
                int row = i / 3; int column = i % 3;
                float left = 55 + column * 330; float top = -380 - row * 82;
                MakeButton(card.transform, capturedPlacement.instanceId, "EDIT " + TitleCase(capturedPlacement.buildingId), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, top - 62), new Vector2(left + 300, top), WorldPrimitiveFactory.Hex("44584B"), () =>
                    {
                        if (world.Placement.BeginMove(capturedPlacement.instanceId)) { CloseModal(); ShowToast("Move selected building, then confirm or cancel"); }
                    });
            }
        }

        private void ShowOrders()
        {
            GameServices services = GameServices.Instance;
            services.Orders.RefreshExpired();
            services.Orders.EnsureOrders();
            GameObject card = OpenModal("VILLAGE ORDER BOARD", 1160, 680);
            Label(card.transform, "Intro", "Fill requests from your neighbors to earn coins and XP.", 20, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0, 1), Vector2.one, new Vector2(40, -135), new Vector2(-40, -90), cream);
            for (int i = 0; i < services.Orders.Active.Count; i++)
            {
                OrderProgress order = services.Orders.Active[i];
                float left = 45 + i * 370;
                GameObject orderCard = Panel(card.transform, order.id, new Color(.18f, .235f, .17f, 1f), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, -545), new Vector2(left + 335, -165));
                Label(orderCard.transform, "Title", TitleCase(order.definitionId), 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one,
                    new Vector2(15, -70), new Vector2(-15, -12), gold);
                Label(orderCard.transform, "Items", OrderItems(order), 21, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0, .42f), new Vector2(1, .84f),
                    new Vector2(25, 5), new Vector2(-25, -5), cream);
                Label(orderCard.transform, "Reward", $"REWARD\n{order.coinReward} coins   +{order.xpReward} XP", 20, FontStyle.Bold, TextAnchor.MiddleCenter,
                    new Vector2(0, .22f), new Vector2(1, .43f), new Vector2(10, 0), new Vector2(-10, 0), Color.white);
                bool completed = order.completed;
                Button deliver = MakeButton(orderCard.transform, "Deliver", completed ? "DELIVERED" : "DELIVER ORDER", Vector2.zero, new Vector2(1, .21f),
                    new Vector2(20, 16), new Vector2(-20, -7), completed ? WorldPrimitiveFactory.Hex("515650") : terracotta, () => DeliverOrder(order));
                deliver.interactable = !completed;
            }
            MakeButton(card.transform, "Market", "SELL EXTRA GOODS AT MARKET", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-245, 22), new Vector2(245, 86),
                green, () => { CloseModal(); world?.Station("Market")?.Interact(world.Player); });
        }

        private void DeliverOrder(OrderProgress order)
        {
            GameServices services = GameServices.Instance;
            if (!services.Orders.Deliver(order.id))
            {
                ShowToast("You still need: " + MissingItems(order));
                return;
            }
            CloseModal();
            ShowToast($"Order delivered: +{order.coinReward} coins, +{order.xpReward} XP");
        }

        private void ShowMap()
        {
            GameServices services = GameServices.Instance;
            GameObject card = OpenModal("PHUM FARM MAP", 1100, 700);
            (string title, string id, string description)[] districts =
            {
                ("Starter Farm", "", "Fields, home and village market"),
                ("Animal Meadow", "animal_meadow", "Chicken, cow and livestock area"),
                ("Production Village", "production_village", "Mill, bakery and workshop"),
                ("Village Market", "", "Sell goods and complete orders"),
                ("River Orchard", "river_orchard", "Fruit trees beside the river"),
                ("Festival Grounds", "festival_grounds", "Seasonal village celebrations")
            };
            for (int i = 0; i < districts.Length; i++)
            {
                var district = districts[i];
                int row = i / 3;
                int column = i % 3;
                float left = 45 + column * 345;
                float top = -140 - row * 242;
                bool unlocked = string.IsNullOrEmpty(district.id) || district.id == "festival_grounds" && services.State.Data.player.level >= 10 || services.Unlocks.Expansion(district.id);
                GameObject tile = Panel(card.transform, district.title, unlocked ? new Color(.20f, .35f, .20f, 1f) : new Color(.20f, .22f, .20f, 1f),
                    new Vector2(0, 1), new Vector2(0, 1), new Vector2(left, top - 215), new Vector2(left + 315, top));
                Label(tile.transform, "Icon", unlocked ? "[ FARM ]" : "[ LOCKED ]", 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .60f), Vector2.one,
                    new Vector2(10, 5), new Vector2(-10, -5), unlocked ? gold : new Color(1, 1, 1, .45f));
                Label(tile.transform, "Title", district.title, 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .34f), new Vector2(1, .62f),
                    new Vector2(12, 0), new Vector2(-12, 0), cream);
                Label(tile.transform, "Description", unlocked ? district.description : DistrictRequirement(district.id), 17, FontStyle.Normal, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(1, .34f), new Vector2(16, 10), new Vector2(-16, -2), new Color(1, 1, 1, .75f));
            }
        }

        private void ShowQuestBook()
        {
            GameSaveData data = GameServices.Instance.State.Data;
            GameObject card = OpenModal("FARM JOURNEY", 1000, 650);
            string[] tutorial = { "Move the camera", "Select a field", "Till the selected field", "Plant rice", "Harvest mature rice", "Open inventory", "Feed the chicken", "Collect an egg", "Complete a village order", "Reach the next level" };
            int step = Math.Min(data.player.tutorialStep, tutorial.Length - 1);
            GameObject main = Panel(card.transform, "MainQuest", new Color(.18f, .26f, .17f, 1f), new Vector2(0, 1), Vector2.one,
                new Vector2(45, -320), new Vector2(-45, -105));
            Label(main.transform, "Chapter", "CHAPTER 1  -  NEW ROOTS", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0, 1), Vector2.one,
                new Vector2(25, -55), new Vector2(-25, -12), gold);
            Label(main.transform, "Task", tutorial[step], 29, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0, .42f), new Vector2(1, .82f),
                new Vector2(25, 0), new Vector2(-25, 0), cream);
            Label(main.transform, "Progress", $"Tutorial step {Math.Min(data.player.tutorialStep + 1, TutorialSystem.StepCount)} / {TutorialSystem.StepCount}    |    Total harvested {data.player.totalHarvested}\nProgress saves automatically after every step.",
                20, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(1, .42f), new Vector2(25, 12), new Vector2(-25, -2), Color.white);
            MakeButton(card.transform, "DailyGoals", "DAILY GOALS", new Vector2(0, 0), new Vector2(.5f, 0),
                new Vector2(45, 90), new Vector2(-15, 180), green, () => ShowJourneyGoals(true));
            MakeButton(card.transform, "Achievements", "ACHIEVEMENTS", new Vector2(.5f, 0), new Vector2(1, 0),
                new Vector2(15, 90), new Vector2(-45, 180), green, () => ShowJourneyGoals(false));
        }

        private void ShowJourneyGoals(bool daily)
        {
            GameServices services = GameServices.Instance;
            GameObject card = OpenModal(daily ? "DAILY GOALS" : "ACHIEVEMENTS", 1050, 680);
            Label(card.transform, "Intro", daily ? "New goals each UTC day. Claim rewards before the day ends." : "Permanent milestones for your farm.",
                20, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0,1), Vector2.one, new Vector2(30,-140), new Vector2(-30,-95), cream);
            JourneyGoal[] goals = services.Journey.Goals(daily);
            for (int i = 0; i < goals.Length; i++)
            {
                JourneyGoal goal = goals[i]; float top = -165 - i * 135;
                Label(card.transform, goal.Id + "Progress", $"{goal.Title}   {Math.Min(goal.Progress, goal.Target)} / {goal.Target}\nReward: {goal.Coins} coins and {goal.Xp} XP",
                    22, FontStyle.Normal, TextAnchor.MiddleLeft, new Vector2(0,1), new Vector2(0,1), new Vector2(50,top-100), new Vector2(730,top), cream);
                MakeButton(card.transform, goal.Id + "Claim", goal.Claimed ? "CLAIMED" : goal.Ready ? "CLAIM" : "IN PROGRESS",
                    new Vector2(1,1), new Vector2(1,1), new Vector2(-275,top-95), new Vector2(-45,top-10), goal.Ready ? green : new Color(.3f,.3f,.3f,1), () =>
                    {
                        if (services.Journey.Claim(goal.Id, daily)) { services.Saves.Save(); ShowJourneyGoals(daily); }
                    });
            }
            MakeButton(card.transform, "Back", "BACK TO JOURNEY", Vector2.zero, new Vector2(1,0), new Vector2(300,25), new Vector2(-300,85), green, ShowQuestBook);
        }

        private void ShowProfile()
        {
            GameSaveData data = GameServices.Instance.State.Data;
            GameObject card = OpenModal("FARMER PROFILE", 860, 620);
            Label(card.transform, "Badge", "PF", 56, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 1), new Vector2(.5f, 1),
                new Vector2(-65, -210), new Vector2(65, -90), gold);
            Label(card.transform, "Name", "PHUM FARMER", 30, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one,
                new Vector2(30, -265), new Vector2(-30, -205), cream);
            string stats = $"Level {data.player.level}    XP {data.player.xp}/{GameServices.Instance.Progression.NextLevelXp}\n" +
                           $"Days farmed {data.world.day}    Crops harvested {data.player.totalHarvested}\n" +
                           $"Coins earned {data.economy.totalEarned:N0}    Orders delivered {data.orders.orders.Count(value => value.completed)}";
            Label(card.transform, "Stats", stats, 22, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0, .28f), new Vector2(1, .62f),
                new Vector2(40, 0), new Vector2(-40, 0), cream);
            MakeButton(card.transform, "Save", "SAVE FARM", Vector2.zero, new Vector2(.5f, .22f), new Vector2(50, 26), new Vector2(-12, -5), green,
                () => { SaveRequested?.Invoke(); ShowToast("Farm saved"); });
            MakeButton(card.transform, "Help", "HOW TO PLAY", new Vector2(.5f, 0), new Vector2(1, .22f), new Vector2(12, 26), new Vector2(-50, -5),
                WorldPrimitiveFactory.Hex("75533A"), ShowHelp);
        }

        private void ShowSettings()
        {
            SettingsState settings = GameServices.Instance.State.Data.settings;
            GameObject card = OpenModal("SETTINGS", 920, 660);
            AddTabStrip(card.transform, new[] { "AUDIO", "GAMEPLAY", "GRAPHICS", "LANGUAGE" }, 0);
            AddSettingRow(card.transform, "Music", "Village soundtrack volume", -205, $"{settings.musicVolume:P0}", () => ChangeVolume(true));
            AddSettingRow(card.transform, "Sound Effects", "Farming and interface sounds", -295, $"{settings.effectsVolume:P0}", () => ChangeVolume(false));
            AddSettingRow(card.transform, "Vibration", "Touch feedback on supported devices", -385, settings.vibration ? "ON" : "OFF", ToggleVibration);
            AddSettingRow(card.transform, "Graphics", "Mobile rendering quality", -475, new[] { "LOW", "MEDIUM", "HIGH" }[Mathf.Clamp(settings.quality, 0, 2)], CycleQuality);
            MakeButton(card.transform, "Language", GameServices.Instance.Localization.Get("language"), new Vector2(.5f, 0), new Vector2(.5f, 0),
                new Vector2(-250, 28), new Vector2(250, 98), green, () =>
                {
                    GameServices.Instance.Localization.Cycle();
                    CloseModal();
                    ShowSettings();
                });
        }

        private void ChangeVolume(bool music)
        {
            SettingsState settings = GameServices.Instance.State.Data.settings;
            float current = music ? settings.musicVolume : settings.effectsVolume;
            float next = current >= .99f ? 0f : Mathf.Clamp01(current + .25f);
            if (music) settings.musicVolume = next; else settings.effectsVolume = next;
            GameServices.Instance.Audio.ApplySettings();
            SaveAndReopenSettings();
        }

        private void ToggleVibration()
        {
            GameServices.Instance.State.Data.settings.vibration = !GameServices.Instance.State.Data.settings.vibration;
            SaveAndReopenSettings();
        }

        private void CycleQuality()
        {
            SettingsState settings = GameServices.Instance.State.Data.settings;
            settings.quality = (settings.quality + 1) % 3;
            QualitySettings.SetQualityLevel(Mathf.Min(settings.quality, QualitySettings.names.Length - 1), true);
            SaveAndReopenSettings();
        }

        private void SaveAndReopenSettings()
        {
            GameServices.Instance.Saves.Save();
            CloseModal();
            ShowSettings();
        }

        private void ShowPauseMenu()
        {
            GameObject card = OpenModal("FARM PAUSED", 680, 650);
            string[] labels = { "RESUME", "SETTINGS", "HOW TO PLAY", "SAVE FARM", "SAVE AND MAIN MENU" };
            Action[] actions =
            {
                CloseModal,
                ShowSettings,
                ShowHelp,
                () => { SaveRequested?.Invoke(); ShowToast("Farm saved"); },
                () => { SaveRequested?.Invoke(); CloseModal(); MainMenuRequested?.Invoke(); }
            };
            for (int i = 0; i < labels.Length; i++)
            {
                int captured = i;
                float top = -115 - i * 92;
                MakeButton(card.transform, labels[i], labels[i], new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-245, top - 72), new Vector2(245, top),
                    i == 0 ? green : i == 4 ? terracotta : WorldPrimitiveFactory.Hex("75533A"), () => actions[captured]());
            }
        }

        private void ShowHelp()
        {
            GameObject card = OpenModal("HOW TO PLAY", 980, 660);
            string help =
                "MOVE\nWASD, arrow keys, or click the ground. Press E near a station or field.\n\n" +
                "FARM\nTill an empty plot, choose a seed, wait for growth, then harvest. Use the well to help crops.\n\n" +
                "VILLAGE\nFeed animals, craft products, fill orders, sell goods, and unlock new districts.\n\n" +
                "SAVE\nUse the menu at any time. The farm also saves when you rest, unlock land, pause the app, or quit.";
            Label(card.transform, "Guide", help, 22, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.one,
                new Vector2(65, 65), new Vector2(-65, -110), cream);
        }

        private void ShowStationScreen(FarmStation station)
        {
            if (station == null) return;
            GameServices services = GameServices.Instance;
            GameObject card = OpenModal(station.Title.ToUpperInvariant(), 820, 560);
            string details = station.Prompt;
            string button = "USE STATION";
            if (station.Kind == StationKind.Production)
            {
                ProductDefinition product = services.Balance.Product(station.Id);
                if (product != null)
                {
                    string ingredients = string.Join("   ", product.inputs.Select(value => $"{TitleCase(value.itemId)} {services.Inventory.Amount(value.itemId)}/{value.quantity}"));
                    details = $"Produces {TitleCase(product.id)}\n\nIngredients: {ingredients}\nTime: {product.seconds:0} seconds    Sell value: {product.sellPrice} coins    +{product.xp} XP\n\nStatus: {station.Prompt}";
                    button = station.Prompt.StartsWith("Collect", StringComparison.OrdinalIgnoreCase) ? "COLLECT" : "START PRODUCTION";
                }
            }
            else if (station.Kind == StationKind.Animal)
            {
                AnimalDefinition animal = services.Balance.Animal(station.Id);
                if (animal != null)
                    details = $"Care for your {TitleCase(animal.id)}\n\nFeed needed: {animal.feedQuantity} Animal Feed\nProduces: {TitleCase(animal.productId)}\nTime: {animal.seconds:0} seconds\n\nStatus: {station.Prompt}";
                button = station.Prompt.StartsWith("Collect", StringComparison.OrdinalIgnoreCase) ? "COLLECT GOODS" : "FEED ANIMAL";
            }
            else if (station.Kind == StationKind.Expansion)
            {
                ExpansionDefinition expansion = services.Balance.Expansion(station.Id);
                details = expansion == null ? station.Prompt : $"Expand Phum Farm into {TitleCase(expansion.id)}.\n\nRequired level: {expansion.requiredLevel}\nLand price: {expansion.coinCost} coins\nYour coins: {services.State.Data.economy.coins}\n\n{station.Prompt}";
                button = "UNLOCK DISTRICT";
            }
            Label(card.transform, "Details", details, 22, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0, .25f), Vector2.one,
                new Vector2(55, 10), new Vector2(-55, -105), cream);
            MakeButton(card.transform, "Action", button, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-260, 48), new Vector2(260, 128), green,
                () => { station.PerformPrimaryAction(); CloseModal(); });
        }

        private void ShowLevelUpScreen(int level)
        {
            GameServices services = GameServices.Instance;
            GameObject card = OpenModal("LEVEL UP!", 760, 580);
            Label(card.transform, "Level", level.ToString(), 92, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 1), new Vector2(.5f, 1),
                new Vector2(-100, -230), new Vector2(100, -100), gold);
            List<string> unlocks = new();
            unlocks.AddRange(services.Balance.crops.Where(value => value.unlockLevel == level).Select(value => TitleCase(value.id) + " crop"));
            unlocks.AddRange(services.Balance.products.Where(value => value.unlockLevel == level).Select(value => TitleCase(value.id) + " recipe"));
            unlocks.AddRange(services.Balance.animals.Where(value => value.unlockLevel == level).Select(value => TitleCase(value.id) + " habitat"));
            unlocks.AddRange(services.Balance.expansions.Where(value => value.requiredLevel == level).Select(value => TitleCase(value.id) + " district"));
            string unlocked = unlocks.Count == 0 ? "More farm space and rewards are now available." : "NEW UNLOCKS\n" + string.Join("  |  ", unlocks);
            Label(card.transform, "Unlocked", unlocked, 23, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .27f), new Vector2(1, .60f),
                new Vector2(45, 0), new Vector2(-45, 0), cream);
            MakeButton(card.transform, "Continue", "KEEP FARMING", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-240, 45), new Vector2(240, 125), green, CloseModal);
        }

        private void AddTabStrip(Transform parent, string[] tabs, int active)
        {
            float cardWidth = ((RectTransform)parent).sizeDelta.x;
            float totalWidth = Mathf.Max(400f, cardWidth - 80f);
            float start = 40f;
            float width = totalWidth / tabs.Length;
            for (int i = 0; i < tabs.Length; i++)
            {
                float left = start + i * width;
                Panel(parent, "TabBackground" + i, i == active ? green : new Color(.15f, .18f, .15f, 1f), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, -150), new Vector2(left + width - 8, -95));
                Label(parent, "Tab" + i, tabs[i], 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left + 5, -148), new Vector2(left + width - 13, -97), Color.white);
            }
        }

        private void AddSettingRow(Transform parent, string title, string description, float top, string value, Action action)
        {
            GameObject row = Panel(parent, title, new Color(.15f, .20f, .15f, 1f), new Vector2(0, 1), Vector2.one,
                new Vector2(50, top - 76), new Vector2(-50, top));
            Label(row.transform, "Label", title + "\n" + description, 19, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one,
                new Vector2(22, 8), new Vector2(-180, -8), cream);
            MakeButton(row.transform, "Toggle", value, new Vector2(1, 0), Vector2.one, new Vector2(-155, 12), new Vector2(-16, -12), green, action);
        }

        private void MakeProgressBar(Transform parent, string name, float value, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject track = Panel(parent, name, new Color(.08f, .09f, .08f, 1f), anchorMin, anchorMax, offsetMin, offsetMax);
            Panel(track.transform, "Fill", green, Vector2.zero, new Vector2(Mathf.Clamp01(value), 1), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
        }

        private static ItemStack Stack(string id, int quantity) => new() { id = id, quantity = quantity };

        private string OrderItems(OrderProgress order) => string.Join("\n", order.requirements.Select(item =>
            $"{TitleCase(item.id),-16} {GameServices.Instance.Inventory.Amount(item.id)} / {item.quantity}"));

        private string MissingItems(OrderProgress order) => string.Join(", ", order.requirements
            .Where(item => GameServices.Instance.Inventory.Amount(item.id) < item.quantity)
            .Select(item => $"{item.quantity - GameServices.Instance.Inventory.Amount(item.id)} {TitleCase(item.id)}"));

        private string DistrictRequirement(string id)
        {
            ExpansionDefinition definition = GameServices.Instance.Balance.Expansion(id);
            return definition == null ? "Unlocks later in your journey" : $"Level {definition.requiredLevel} and {definition.coinCost} coins";
        }

        private static string ItemGlyph(string id) => id switch
        {
            "rice" => "RICE", "corn" => "CORN", "tomato" => "TOMATO", "sugarcane" => "CANE",
            "egg" => "EGG", "milk" => "MILK", "animal_feed" => "FEED", "bread" => "BREAD",
            "butter" => "BUTTER", "palm_sugar" => "SUGAR", _ => "ITEM"
        };

        private static string BuildingGlyph(string id) => id.Contains("path") ? "PATH" : id.Contains("lantern") ? "LAMP" : id.Contains("garden") || id.Contains("grove") ? "TREE" : "HOUSE";
    }
}
