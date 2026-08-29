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
            (string id, string title, int price, int level)[] offers =
            {
                ("chicken_coop", "Chicken Coop", 180, 2), ("rice_field", "Rice Field", 80, 1),
                ("banana_grove", "Banana Grove", 260, 3), ("lotus_lantern", "Lotus Lantern", 95, 2),
                ("cow_shelter", "Cow Shelter", 520, 4), ("stone_path", "Stone Path", 45, 2),
                ("bakery", "Bakery", 760, 5), ("palm_garden", "Palm Garden", 310, 3)
            };
            for (int i = 0; i < offers.Length; i++)
            {
                var offer = offers[i];
                int row = i / 4;
                int column = i % 4;
                float left = 42 + column * 275;
                float top = -180 - row * 228;
                bool unlocked = services.Unlocks.Level(offer.level);
                GameObject offerCard = Panel(card.transform, offer.id, new Color(.18f, .235f, .17f, 1f), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, top - 204), new Vector2(left + 255, top));
                Label(offerCard.transform, "Art", BuildingGlyph(offer.id), 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .54f), Vector2.one,
                    new Vector2(8, 2), new Vector2(-8, -6), gold);
                Label(offerCard.transform, "Name", offer.title, 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, .32f), new Vector2(1, .55f),
                    new Vector2(8, 0), new Vector2(-8, 0), cream);
                string priceText = unlocked ? $"{offer.price} COINS" : $"UNLOCKS LEVEL {offer.level}";
                Button buy = MakeButton(offerCard.transform, "Select", priceText, Vector2.zero, new Vector2(1, .30f), new Vector2(12, 12), new Vector2(-12, -4),
                    unlocked ? green : WorldPrimitiveFactory.Hex("515650"), () => SelectShopOffer(offer.title));
                buy.interactable = unlocked;
            }
        }

        private void SelectShopOffer(string title)
        {
            CloseModal();
            ShowToast($"{title} selected for build mode");
            ShowBuildMode();
        }

        private void ShowBuildMode()
        {
            GameObject card = OpenModal("BUILD MODE", 980, 480);
            Label(card.transform, "Help", "Choose an item in the Village Shop, then arrange it on your farm.\nYour current world remains safe until you confirm.",
                23, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one, new Vector2(60, -190), new Vector2(-60, -95), cream);
            string[] labels = { "MOVE", "ROTATE", "STORE", "CONFIRM" };
            string[] messages = { "Drag the selected building to move it", "Building rotated 90 degrees", "Building returned to storage", "Farm layout saved" };
            for (int i = 0; i < labels.Length; i++)
            {
                int captured = i;
                float left = 44 + i * 225;
                MakeButton(card.transform, labels[i], labels[i], Vector2.zero, new Vector2(0, 1), new Vector2(left, 92), new Vector2(left + 205, 166),
                    i == 3 ? green : WorldPrimitiveFactory.Hex("75533A"), () =>
                    {
                        ShowToast(messages[captured]);
                        if (captured == 3) { GameServices.Instance.Saves.Save(); CloseModal(); }
                    });
            }
            Label(card.transform, "Note", "FBX and GLB models can be dropped into Assets/PhumFarm/Art/Models later without replacing these systems.", 18,
                FontStyle.Italic, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1, 0), new Vector2(35, 22), new Vector2(-35, 70), new Color(1, 1, 1, .65f));
        }

        private void ShowOrders()
        {
            GameObject card = OpenModal("VILLAGE ORDER BOARD", 1160, 680);
            Label(card.transform, "Intro", "Fill requests from your neighbors to earn coins and XP.", 20, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0, 1), Vector2.one, new Vector2(40, -135), new Vector2(-40, -90), cream);
            for (int i = 0; i < FarmOrders.Length; i++)
            {
                FarmOrder order = FarmOrders[i];
                float left = 45 + i * 370;
                GameObject orderCard = Panel(card.transform, order.Id, new Color(.18f, .235f, .17f, 1f), new Vector2(0, 1), new Vector2(0, 1),
                    new Vector2(left, -545), new Vector2(left + 335, -165));
                Label(orderCard.transform, "Title", order.Title, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one,
                    new Vector2(15, -70), new Vector2(-15, -12), gold);
                Label(orderCard.transform, "Items", OrderItems(order), 21, FontStyle.Normal, TextAnchor.UpperLeft, new Vector2(0, .42f), new Vector2(1, .84f),
                    new Vector2(25, 5), new Vector2(-25, -5), cream);
                Label(orderCard.transform, "Reward", $"REWARD\n{order.Coins} coins   +{order.Xp} XP", 20, FontStyle.Bold, TextAnchor.MiddleCenter,
                    new Vector2(0, .22f), new Vector2(1, .43f), new Vector2(10, 0), new Vector2(-10, 0), Color.white);
                bool completed = IsOrderCompleted(order.Id);
                Button deliver = MakeButton(orderCard.transform, "Deliver", completed ? "DELIVERED" : "DELIVER ORDER", Vector2.zero, new Vector2(1, .21f),
                    new Vector2(20, 16), new Vector2(-20, -7), completed ? WorldPrimitiveFactory.Hex("515650") : terracotta, () => DeliverOrder(order));
                deliver.interactable = !completed;
            }
            MakeButton(card.transform, "Market", "SELL EXTRA GOODS AT MARKET", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-245, 22), new Vector2(245, 86),
                green, () => { CloseModal(); world?.Station("Market")?.Interact(world.Player); });
        }

        private void DeliverOrder(FarmOrder order)
        {
            GameServices services = GameServices.Instance;
            if (!services.Inventory.Remove(order.Items))
            {
                ShowToast("You still need: " + MissingItems(order));
                return;
            }
            OrderProgress progress = services.State.Data.orders.orders.Find(value => value.id == order.Id);
            if (progress == null)
            {
                progress = new OrderProgress { id = order.Id };
                services.State.Data.orders.orders.Add(progress);
            }
            progress.completed = true;
            services.Economy.AddCoins(order.Coins);
            services.Progression.AddXp(order.Xp);
            services.Saves.Save();
            CloseModal();
            ShowToast($"{order.Title} delivered: +{order.Coins} coins, +{order.Xp} XP");
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
            string[] tutorial = { "Till a field", "Plant your first crop", "Harvest when it is ready", "Sell goods at the village market", "Grow the village and unlock districts" };
            int step = Math.Min(data.player.tutorialStep, tutorial.Length - 1);
            GameObject main = Panel(card.transform, "MainQuest", new Color(.18f, .26f, .17f, 1f), new Vector2(0, 1), Vector2.one,
                new Vector2(45, -320), new Vector2(-45, -105));
            Label(main.transform, "Chapter", "CHAPTER 1  -  NEW ROOTS", 18, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0, 1), Vector2.one,
                new Vector2(25, -55), new Vector2(-25, -12), gold);
            Label(main.transform, "Task", tutorial[step], 29, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0, .42f), new Vector2(1, .82f),
                new Vector2(25, 0), new Vector2(-25, 0), cream);
            Label(main.transform, "Progress", $"Tutorial step {Math.Min(data.player.tutorialStep + 1, 5)} / 5    |    Total harvested {data.player.totalHarvested}\nReward: village progress and XP",
                20, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(1, .42f), new Vector2(25, 12), new Vector2(-25, -2), Color.white);
            Label(card.transform, "DailyTitle", "DAILY GOALS", 22, FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0, 1), Vector2.one,
                new Vector2(48, -375), new Vector2(-48, -330), gold);
            string daily = $"Harvest 5 crops             {Math.Min(data.player.totalHarvested, 5)} / 5\nEarn 250 coins              {Math.Min(data.economy.totalEarned, 250)} / 250\nComplete a village order    {data.orders.orders.Count(value => value.completed)} / 1";
            Label(card.transform, "Daily", daily, 21, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(1, .40f),
                new Vector2(55, 5), new Vector2(-55, -5), cream);
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
            GameObject card = OpenModal("SETTINGS", 920, 660);
            AddTabStrip(card.transform, new[] { "AUDIO", "GAMEPLAY", "GRAPHICS", "LANGUAGE" }, 0);
            AddSettingRow(card.transform, "Music", "Village soundtrack", -205, "ON", "Music setting changed");
            AddSettingRow(card.transform, "Sound Effects", "Farming and interface sounds", -295, "ON", "Sound effects setting changed");
            AddSettingRow(card.transform, "Weather", "Show changing farm weather", -385, "ON", "Weather setting changed");
            AddSettingRow(card.transform, "Camera", "Smooth isometric follow", -475, "SMOOTH", "Camera setting changed");
            MakeButton(card.transform, "Language", GameServices.Instance.Localization.Get("language"), new Vector2(.5f, 0), new Vector2(.5f, 0),
                new Vector2(-250, 28), new Vector2(250, 98), green, () =>
                {
                    GameServices.Instance.Localization.Cycle();
                    CloseModal();
                    ShowSettings();
                });
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

        private void AddSettingRow(Transform parent, string title, string description, float top, string value, string message)
        {
            GameObject row = Panel(parent, title, new Color(.15f, .20f, .15f, 1f), new Vector2(0, 1), Vector2.one,
                new Vector2(50, top - 76), new Vector2(-50, top));
            Label(row.transform, "Label", title + "\n" + description, 19, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one,
                new Vector2(22, 8), new Vector2(-180, -8), cream);
            MakeButton(row.transform, "Toggle", value, new Vector2(1, 0), Vector2.one, new Vector2(-155, 12), new Vector2(-16, -12), green, () => ShowToast(message));
        }

        private void MakeProgressBar(Transform parent, string name, float value, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject track = Panel(parent, name, new Color(.08f, .09f, .08f, 1f), anchorMin, anchorMax, offsetMin, offsetMax);
            Panel(track.transform, "Fill", green, Vector2.zero, new Vector2(Mathf.Clamp01(value), 1), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
        }

        private static ItemStack Stack(string id, int quantity) => new() { id = id, quantity = quantity };

        private bool IsOrderCompleted(string id) => GameServices.Instance.State.Data.orders.orders.Find(value => value.id == id)?.completed ?? false;

        private string OrderItems(FarmOrder order) => string.Join("\n", order.Items.Select(item =>
            $"{TitleCase(item.id),-16} {GameServices.Instance.Inventory.Amount(item.id)} / {item.quantity}"));

        private string MissingItems(FarmOrder order) => string.Join(", ", order.Items
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
