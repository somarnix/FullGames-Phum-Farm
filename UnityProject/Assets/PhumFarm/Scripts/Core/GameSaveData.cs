using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhumFarm.Core
{
    [Serializable] public sealed class ItemStack { public string id = string.Empty; public int quantity; }
    [Serializable] public sealed class PlayerState { public int level = 1; public int xp; public int tutorialStep; public int totalHarvested; }
    [Serializable] public sealed class EconomyState { public int coins = 120; public int gems = 10; public int totalEarned; }
    [Serializable] public sealed class InventoryState { public List<ItemStack> items = new(); }
    [Serializable] public sealed class PlotState { public string state = "empty"; public string cropId = string.Empty; public double plantedAt; }
    [Serializable] public sealed class CropState { public List<PlotState> plots = new(); }
    [Serializable] public sealed class AnimalJob { public string animalId = string.Empty; public bool fed; public double readyAt; }
    [Serializable] public sealed class AnimalState { public List<AnimalJob> jobs = new(); }
    [Serializable] public sealed class ProductionJob { public string productId = string.Empty; public double startedAt; public double readyAt; }
    [Serializable] public sealed class ProductionState { public List<ProductionJob> jobs = new(); }
    [Serializable] public sealed class BuildingPlacement { public string buildingId = string.Empty; public Vector3 position; public float rotationY; }
    [Serializable] public sealed class BuildingState { public List<BuildingPlacement> placements = new(); }
    [Serializable] public sealed class ExpansionState { public string id = string.Empty; public bool unlocked; }
    [Serializable] public sealed class WorldState
    {
        public int day = 1;
        public float minutes = 420f;
        public string weather = "clear";
        public string selectedCrop = "rice";
        public List<ExpansionState> expansions = new();
    }
    [Serializable] public sealed class QuestProgress { public string id = string.Empty; public int progress; public bool completed; }
    [Serializable] public sealed class QuestState { public List<QuestProgress> quests = new(); }
    [Serializable] public sealed class OrderProgress { public string id = string.Empty; public bool completed; }
    [Serializable] public sealed class OrderState { public List<OrderProgress> orders = new(); }

    [Serializable]
    public sealed class GameSaveData
    {
        public int saveVersion = SaveManager.CurrentSaveVersion;
        public long savedAtUnix;
        public PlayerState player = new();
        public EconomyState economy = new();
        public InventoryState inventory = new();
        public WorldState world = new();
        public BuildingState buildings = new();
        public CropState crops = new();
        public AnimalState animals = new();
        public ProductionState production = new();
        public QuestState quests = new();
        public OrderState orders = new();
    }
}
