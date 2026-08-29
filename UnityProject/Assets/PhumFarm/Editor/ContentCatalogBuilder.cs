using System;
using System.Collections.Generic;
using System.IO;
using PhumFarm.Data;
using UnityEditor;
using UnityEngine;

namespace PhumFarm.Editor
{
    [InitializeOnLoad]
    public static class ContentCatalogBuilder
    {
        private const string Root = "Assets/PhumFarm/Resources/Definitions";
        static ContentCatalogBuilder() => EditorApplication.delayCall += EnsureContent;

        [MenuItem("Phum Farm/Rebuild Content Definitions")]
        public static void EnsureContent()
        {
            EnsureFolder(Root);
            var catalog = LoadOrCreate<GameContentCatalog>(Root + "/GameContentCatalog.asset");
            catalog.items = BuildItems();
            catalog.crops = BuildCrops();
            catalog.animals = BuildAnimals();
            catalog.recipes = BuildRecipes();
            catalog.buildings = BuildBuildings();
            catalog.expansions = BuildExpansions();
            catalog.levels = BuildLevels();
            catalog.orders = BuildOrders();
            catalog.quests = BuildQuests();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        private static List<ItemDefinitionAsset> BuildItems()
        {
            (string id, ItemCategory category, int price)[] rows =
            {
                ("wheat", ItemCategory.Crop, 7), ("rice", ItemCategory.Crop, 8), ("corn", ItemCategory.Crop, 14), ("tomato", ItemCategory.Crop, 23),
                ("sugarcane", ItemCategory.Crop, 34), ("egg", ItemCategory.AnimalProduct, 18), ("milk", ItemCategory.AnimalProduct, 28),
                ("truffle", ItemCategory.AnimalProduct, 65), ("wool", ItemCategory.AnimalProduct, 48), ("goat_milk", ItemCategory.AnimalProduct, 38),
                ("flour", ItemCategory.CraftedGood, 18), ("animal_feed", ItemCategory.CraftedGood, 20), ("bread", ItemCategory.CraftedGood, 38),
                ("butter", ItemCategory.CraftedGood, 52), ("palm_sugar", ItemCategory.CraftedGood, 82)
            };
            var result = new List<ItemDefinitionAsset>();
            foreach (var row in rows) result.Add(Definition<ItemDefinitionAsset>("Items", row.id, value =>
            {
                value.id = row.id; value.displayNameKey = "item." + row.id; value.category = row.category; value.sellPrice = row.price;
            }));
            return result;
        }

        private static List<CropDefinitionAsset> BuildCrops()
        {
            (string id, int level, int cost, float time, int amount, int xp, int sell, string color)[] rows =
            {
                ("wheat", 1, 2, 34, 3, 7, 7, "E6BB45"), ("rice", 1, 2, 38, 3, 7, 8, "E9C94F"), ("corn", 2, 4, 55, 3, 10, 14, "F0B83D"),
                ("tomato", 4, 7, 72, 3, 14, 23, "DC594B"), ("sugarcane", 6, 10, 95, 4, 19, 34, "A8D86D")
            };
            var result = new List<CropDefinitionAsset>();
            foreach (var row in rows) result.Add(Definition<CropDefinitionAsset>("Crops", row.id, value =>
            {
                value.id = row.id; value.displayNameKey = "crop." + row.id; value.unlockLevel = row.level; value.seedCost = row.cost;
                value.growTime = row.time; value.harvestAmount = row.amount; value.xpReward = row.xp; value.sellPrice = row.sell;
                if (ColorUtility.TryParseHtmlString("#" + row.color, out Color color)) value.fallbackColor = color;
            }));
            return result;
        }

        private static List<AnimalDefinitionAsset> BuildAnimals()
        {
            (string id, int level, int feed, float time, string product, int amount, int xp)[] rows =
            {
                ("chicken", 2, 1, 40, "egg", 2, 10), ("cow", 4, 1, 65, "milk", 2, 16),
                ("buffalo", 6, 2, 80, "milk", 3, 22), ("pig", 8, 2, 90, "truffle", 1, 28),
                ("sheep", 9, 2, 100, "wool", 2, 30), ("goat", 9, 2, 95, "goat_milk", 2, 29)
            };
            var result = new List<AnimalDefinitionAsset>();
            foreach (var row in rows) result.Add(Definition<AnimalDefinitionAsset>("Animals", row.id, value =>
            {
                value.id = row.id; value.displayNameKey = "animal." + row.id; value.unlockLevel = row.level; value.feedItem = "animal_feed";
                value.feedAmount = row.feed; value.productionTime = row.time; value.productItem = row.product; value.productAmount = row.amount; value.xpReward = row.xp;
            }));
            return result;
        }

        private static List<RecipeDefinitionAsset> BuildRecipes()
        {
            (string id, int level, float time, int sell, int xp, string input, int amount)[] rows =
            {
                ("flour", 2, 20, 18, 8, "wheat", 2), ("animal_feed", 1, 18, 20, 8, "rice", 2), ("bread", 3, 28, 38, 14, "rice", 3),
                ("butter", 5, 34, 52, 18, "milk", 2), ("palm_sugar", 7, 42, 82, 25, "sugarcane", 3)
            };
            var result = new List<RecipeDefinitionAsset>();
            foreach (var row in rows) result.Add(Definition<RecipeDefinitionAsset>("Recipes", row.id, value =>
            {
                value.id = row.id; value.displayNameKey = "product." + row.id; value.unlockLevel = row.level; value.productionTime = row.time;
                value.sellPrice = row.sell; value.xpReward = row.xp; value.ingredients = new[] { new ItemRequirement { itemId = row.input, quantity = row.amount } };
            }));
            return result;
        }

        private static List<BuildingDefinitionAsset> BuildBuildings()
        {
            (string id, int level, int cost, Vector2Int size, string production, string color)[] rows =
            {
                ("farm_house",1,0,new Vector2Int(3,3),"","F2C879"), ("barn",1,0,new Vector2Int(3,2),"inventory","C99653"),
                ("well",1,40,new Vector2Int(1,1),"water","79C7D7"), ("feed_mill",1,90,new Vector2Int(2,2),"animal_feed","D4A356"),
                ("chicken_coop",2,120,new Vector2Int(2,2),"chicken","E7B963"), ("bakery",3,220,new Vector2Int(2,2),"bread","D9865F"),
                ("cow_barn",4,340,new Vector2Int(3,2),"cow","A27150"), ("dairy",5,480,new Vector2Int(2,2),"butter","E8E0C9"),
                ("buffalo_shelter",6,650,new Vector2Int(3,2),"buffalo","77705D"), ("sugar_workshop",7,820,new Vector2Int(2,2),"palm_sugar","B98352")
            };
            var result = new List<BuildingDefinitionAsset>();
            foreach (var row in rows) result.Add(Definition<BuildingDefinitionAsset>("Buildings", row.id, value =>
            {
                value.id = row.id; value.displayNameKey = "building." + row.id; value.unlockLevel = row.level; value.cost = row.cost;
                value.size = row.size; value.productionType = row.production; value.constructionTime = row.cost == 0 ? 0 : 10;
                if (ColorUtility.TryParseHtmlString("#" + row.color, out Color color)) value.fallbackColor = color;
            }));
            return result;
        }

        private static List<ExpansionDefinitionAsset> BuildExpansions()
        {
            (string id, int level, int cost, Vector3 center)[] rows =
            {
                ("animal_meadow",3,220,new Vector3(-16,0,8)), ("production_village",5,600,new Vector3(16,0,8)), ("river_orchard",8,1400,new Vector3(0,0,24))
            };
            var result = new List<ExpansionDefinitionAsset>();
            foreach (var row in rows) result.Add(Definition<ExpansionDefinitionAsset>("Expansions", row.id, value =>
            {
                value.id = row.id; value.displayNameKey = "district." + row.id; value.requiredLevel = row.level; value.coinCost = row.cost;
                value.buildBounds = new Bounds(row.center, new Vector3(14, 2, 14));
            }));
            return result;
        }

        private static List<LevelDefinitionAsset> BuildLevels()
        {
            int[] thresholds = { 0, 45, 115, 210, 335, 500, 710, 970, 1280, 1650, 2100 };
            var result = new List<LevelDefinitionAsset>();
            for (int index = 0; index < thresholds.Length; index++)
            {
                int level = index + 1;
                result.Add(Definition<LevelDefinitionAsset>("Levels", level.ToString("00"), value =>
                {
                    value.level = level; value.xpThreshold = thresholds[index]; value.coinReward = level == 1 ? 0 : 35 + level * 15;
                }));
            }
            return result;
        }

        private static List<OrderDefinitionAsset> BuildOrders()
        {
            var result = new List<OrderDefinitionAsset>();
            string[] ids = { "crop_delivery", "animal_delivery", "village_feast" };
            for (int index = 0; index < ids.Length; index++)
            {
                int slot = index;
                result.Add(Definition<OrderDefinitionAsset>("Orders", ids[index], value =>
                {
                    value.id = ids[slot]; value.displayNameKey = "order." + ids[slot]; value.unlockLevel = 1 + slot * 2;
                    value.coinReward = 30 + slot * 35; value.xpReward = 12 + slot * 10; value.refreshSeconds = 120;
                    value.requirements = new[] { new OrderRequirement { itemId = slot == 0 ? "rice" : slot == 1 ? "egg" : "bread", minimum = 2, maximum = 5 } };
                }));
            }
            return result;
        }

        private static List<QuestDefinitionAsset> BuildQuests()
        {
            string[] events = { "move", "till", "plant", "water", "harvest", "open_barn", "feed_animal", "collect_animal", "start_production", "deliver_order" };
            var result = new List<QuestDefinitionAsset>();
            for (int index = 0; index < events.Length; index++)
            {
                int step = index;
                result.Add(Definition<QuestDefinitionAsset>("Quests", $"tutorial_{index + 1:00}", value =>
                {
                    value.id = $"tutorial_{step + 1:00}"; value.displayNameKey = "tutorial." + events[step]; value.eventId = events[step];
                    value.target = 1; value.coinReward = step == events.Length - 1 ? 50 : 5; value.xpReward = 3;
                }));
            }
            return result;
        }

        private static T Definition<T>(string folder, string id, Action<T> configure) where T : ScriptableObject
        {
            string path = Root + "/" + folder;
            EnsureFolder(path);
            T value = LoadOrCreate<T>(path + "/" + id + ".asset");
            configure(value);
            EditorUtility.SetDirty(value);
            return value;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T value = AssetDatabase.LoadAssetAtPath<T>(path);
            if (value != null) return value;
            if (AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
            value = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(value, path);
            return value;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }
    }
}
