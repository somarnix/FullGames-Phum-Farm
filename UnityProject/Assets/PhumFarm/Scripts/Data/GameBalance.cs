using System;
using System.Collections.Generic;
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

            return balance;
        }

        public CropDefinition Crop(string id) => Array.Find(crops, value => value.id == id);
        public ProductDefinition Product(string id) => Array.Find(products, value => value.id == id);
        public AnimalDefinition Animal(string id) => Array.Find(animals, value => value.id == id);
        public ExpansionDefinition Expansion(string id) => Array.Find(expansions, value => value.id == id);
    }
}
