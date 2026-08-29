using System;
using System.Linq;
using PhumFarm.Data;

namespace PhumFarm.Core
{
    public sealed class GameState
    {
        private readonly GameBalance balance;

        public GameSaveData Data { get; private set; }
        public event Action Changed;
        public event Action<int> LevelUp;
        public event Action<string> MessageRequested;

        public GameState(GameBalance balance)
        {
            this.balance = balance;
            Data = CreateDefault();
        }

        public GameSaveData CreateDefault()
        {
            var value = new GameSaveData();
            value.economy.coins = balance.startingCoins;
            value.economy.gems = balance.startingGems;
            foreach (string itemId in new[] { "rice", "corn", "tomato", "sugarcane", "egg", "milk", "animal_feed", "bread", "butter", "palm_sugar" })
            {
                value.inventory.items.Add(new ItemStack { id = itemId, quantity = itemId == "rice" ? 4 : itemId == "animal_feed" ? 2 : 0 });
            }
            for (int index = 0; index < 16; index++) value.crops.plots.Add(new PlotState());
            foreach (AnimalDefinition animal in balance.animals) value.animals.jobs.Add(new AnimalJob { animalId = animal.id });
            foreach (ExpansionDefinition expansion in balance.expansions) value.world.expansions.Add(new ExpansionState { id = expansion.id });
            return value;
        }

        public void NewGame()
        {
            Data = CreateDefault();
            NotifyChanged();
        }

        public void Replace(GameSaveData loaded)
        {
            Data = loaded ?? CreateDefault();
            Normalize();
            NotifyChanged();
        }

        public void Normalize()
        {
            Data.player ??= new PlayerState();
            Data.economy ??= new EconomyState();
            Data.inventory ??= new InventoryState();
            Data.world ??= new WorldState();
            Data.buildings ??= new BuildingState();
            Data.crops ??= new CropState();
            Data.animals ??= new AnimalState();
            Data.production ??= new ProductionState();
            Data.quests ??= new QuestState();
            Data.orders ??= new OrderState();
            Data.player.level = Math.Max(1, Data.player.level);
            Data.player.xp = Math.Max(0, Data.player.xp);
            Data.economy.coins = Math.Max(0, Data.economy.coins);
            Data.economy.gems = Math.Max(0, Data.economy.gems);
            while (Data.crops.plots.Count < 16) Data.crops.plots.Add(new PlotState());
            foreach (string itemId in new[] { "rice", "corn", "tomato", "sugarcane", "egg", "milk", "animal_feed", "bread", "butter", "palm_sugar" })
            {
                if (Data.inventory.items.All(item => item.id != itemId)) Data.inventory.items.Add(new ItemStack { id = itemId });
            }
            foreach (AnimalDefinition animal in balance.animals)
            {
                if (Data.animals.jobs.All(job => job.animalId != animal.id)) Data.animals.jobs.Add(new AnimalJob { animalId = animal.id });
            }
            foreach (ExpansionDefinition expansion in balance.expansions)
            {
                if (Data.world.expansions.All(value => value.id != expansion.id)) Data.world.expansions.Add(new ExpansionState { id = expansion.id });
            }
        }

        public double GameMinutes => Data.world.day * 1440d + Data.world.minutes;

        public void AdvanceTime(float realSeconds)
        {
            Data.world.minutes += Math.Max(0f, realSeconds) * 3f;
            if (Data.world.minutes < 1440f) return;
            Data.world.minutes %= 1440f;
            Data.world.day++;
            GameServices.Instance?.Saves.Save();
        }

        public string ClockText()
        {
            int minutes = (int)Data.world.minutes;
            return $"{minutes / 60:00}:{minutes % 60:00}";
        }

        public void NotifyChanged() => Changed?.Invoke();
        public void NotifyLevelUp(int level) => LevelUp?.Invoke(level);
        public void NotifyMessage(string message) => MessageRequested?.Invoke(message);
    }
}
