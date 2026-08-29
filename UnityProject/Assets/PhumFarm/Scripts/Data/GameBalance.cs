using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhumFarm.Data
{
    [Serializable]
    public sealed class ItemRequirement
    {
        public string itemId = string.Empty;
        public int quantity;
    }

    [Serializable]
    public sealed class CropDefinition
    {
        public string id = string.Empty;
        public string nameKey = string.Empty;
        public int seedCost;
        public int sellPrice;
        public float growSeconds;
        public int xp;
        public int unlockLevel;
        public string color = "73AD4B";
    }

    [Serializable]
    public sealed class ProductDefinition
    {
        public string id = string.Empty;
        public string nameKey = string.Empty;
        public float seconds;
        public int sellPrice;
        public int xp;
        public int unlockLevel;
        public ItemRequirement[] inputs = Array.Empty<ItemRequirement>();
    }

    [Serializable]
    public sealed class AnimalDefinition
    {
        public string id = string.Empty;
        public string productId = string.Empty;
        public int feedQuantity;
        public float seconds;
        public int unlockLevel;
    }

    [Serializable]
    public sealed class ExpansionDefinition
    {
        public string id = string.Empty;
        public string nameKey = string.Empty;
        public int requiredLevel;
        public int coinCost;
    }

    [Serializable]
    public sealed class GameBalance
    {
        public CropDefinition[] crops = Array.Empty<CropDefinition>();
        public ProductDefinition[] products = Array.Empty<ProductDefinition>();
        public AnimalDefinition[] animals = Array.Empty<AnimalDefinition>();
        public ExpansionDefinition[] expansions = Array.Empty<ExpansionDefinition>();
        public int[] levelThresholds = Array.Empty<int>();
        public int[] plotUnlockLevels = Array.Empty<int>();
        public int startingCoins = 120;
        public int startingGems = 10;
        public int barnCapacity = 75;
        [NonSerialized] public GameContentCatalog catalog;

        public static GameBalance Load()
        {
            TextAsset asset = Resources.Load<TextAsset>("Data/game_balance");
            if (asset == null)
            {
                throw new InvalidOperationException("Missing Resources/Data/game_balance.json");
            }

            GameBalance balance = JsonUtility.FromJson<GameBalance>(asset.text);
            if (balance == null || balance.crops.Length == 0 || balance.levelThresholds.Length == 0)
            {
                throw new InvalidOperationException("Invalid Phum Farm balance data");
            }

            balance.catalog = Resources.Load<GameContentCatalog>("Definitions/GameContentCatalog");
            if (balance.catalog != null) balance.ApplyCatalog();
            return balance;
        }

        private void ApplyCatalog()
        {
            if (catalog.crops.Count > 0)
                crops = catalog.crops.Where(value => value != null).Select(value => new CropDefinition
                {
                    id = value.id,
                    nameKey = value.displayNameKey,
                    seedCost = value.seedCost,
                    sellPrice = value.sellPrice,
                    growSeconds = value.growTime,
                    xp = value.xpReward,
                    unlockLevel = value.unlockLevel,
                    color = ColorUtility.ToHtmlStringRGB(value.fallbackColor)
                }).ToArray();
            if (catalog.recipes.Count > 0)
                products = catalog.recipes.Where(value => value != null).Select(value => new ProductDefinition
                {
                    id = value.id,
                    nameKey = value.displayNameKey,
                    seconds = value.productionTime,
                    sellPrice = value.sellPrice,
                    xp = value.xpReward,
                    unlockLevel = value.unlockLevel,
                    inputs = value.ingredients
                }).ToArray();
            if (catalog.animals.Count > 0)
                animals = catalog.animals.Where(value => value != null).Select(value => new AnimalDefinition
                {
                    id = value.id,
                    productId = value.productItem,
                    feedQuantity = value.feedAmount,
                    seconds = value.productionTime,
                    unlockLevel = value.unlockLevel
                }).ToArray();
            if (catalog.expansions.Count > 0)
                expansions = catalog.expansions.Where(value => value != null).Select(value => new ExpansionDefinition
                {
                    id = value.id,
                    nameKey = value.displayNameKey,
                    requiredLevel = value.requiredLevel,
                    coinCost = value.coinCost
                }).ToArray();
            if (catalog.levels.Count > 0)
                levelThresholds = catalog.levels.Where(value => value != null).OrderBy(value => value.level).Select(value => value.xpThreshold).ToArray();
        }

        public CropDefinition Crop(string id) => Array.Find(crops, value => value.id == id);
        public ProductDefinition Product(string id) => Array.Find(products, value => value.id == id);
        public AnimalDefinition Animal(string id) => Array.Find(animals, value => value.id == id);
        public ExpansionDefinition Expansion(string id) => Array.Find(expansions, value => value.id == id);
    }
}
