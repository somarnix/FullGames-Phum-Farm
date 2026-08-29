using System;
using System.Collections.Generic;
using System.Linq;
using PhumFarm.Data;

namespace PhumFarm.Core
{
    public sealed class OrderSystem
    {
        private readonly GameState state;
        private readonly GameBalance balance;
        private readonly InventorySystem inventory;
        private readonly EconomySystem economy;
        private readonly ProgressionSystem progression;

        public OrderSystem(GameState state, GameBalance balance, InventorySystem inventory, EconomySystem economy, ProgressionSystem progression)
        {
            this.state = state;
            this.balance = balance;
            this.inventory = inventory;
            this.economy = economy;
            this.progression = progression;
        }

        public IReadOnlyList<OrderProgress> Active => state.Data.orders.orders;

        public void EnsureOrders(int count = 3)
        {
            state.Data.orders.orders.RemoveAll(value => value == null);
            while (state.Data.orders.orders.Count < count) state.Data.orders.orders.Add(Generate(state.Data.orders.orders.Count));
        }

        public void RefreshExpired()
        {
            for (int index = 0; index < state.Data.orders.orders.Count; index++)
            {
                OrderProgress order = state.Data.orders.orders[index];
                if (order.completed && state.GameMinutes >= order.refreshAt) state.Data.orders.orders[index] = Generate(index);
            }
        }

        public bool CanDeliver(string id)
        {
            OrderProgress order = state.Data.orders.orders.Find(value => value.id == id);
            return order != null && !order.completed && inventory.Has(order.requirements);
        }

        public bool Deliver(string id)
        {
            OrderProgress order = state.Data.orders.orders.Find(value => value.id == id);
            if (order == null || order.completed || !inventory.Remove(order.requirements)) return false;
            order.completed = true;
            GameServices.Instance?.Journey?.RecordOrder();
            order.refreshAt = state.GameMinutes + 6d;
            economy.AddCoins(order.coinReward);
            progression.AddXp(order.xpReward);
            GameServices.Instance?.Tutorial.Record(TutorialAction.CompleteOrder);
            GameServices.Instance?.Saves.Save();
            return true;
        }

        public void RefreshAll()
        {
            state.Data.orders.orders.Clear();
            state.Data.orders.generation++;
            EnsureOrders();
            state.NotifyChanged();
        }

        private OrderProgress Generate(int slot)
        {
            int generation = state.Data.orders.generation++;
            CropDefinition[] unlocked = balance.crops.Where(value => value.unlockLevel <= state.Data.player.level).ToArray();
            CropDefinition crop = unlocked.Length == 0 ? balance.crops[0] : unlocked[Math.Abs(generation + slot) % unlocked.Length];
            int quantity = 2 + Math.Abs(generation * 3 + slot) % 4;
            return new OrderProgress
            {
                id = $"order_{generation}_{slot}",
                definitionId = "crop_delivery",
                requirements = new List<ItemStack> { new() { id = crop.id, quantity = quantity } },
                coinReward = crop.sellPrice * quantity + 12,
                xpReward = crop.xp * quantity,
                refreshAt = state.GameMinutes + 6d
            };
        }
    }
}
