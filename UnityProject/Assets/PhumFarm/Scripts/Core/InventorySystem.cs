using System;
using System.Collections.Generic;
using System.Linq;
using PhumFarm.Data;

namespace PhumFarm.Core
{
    public sealed class InventorySystem
    {
        private readonly GameState state;
        private readonly int defaultCapacity;
        public event Action<string, int> Changed;

        public InventorySystem(GameState state, int capacity) { this.state = state; defaultCapacity = Math.Max(1, capacity); }
        public int Amount(string id) => state.Data.inventory.items.Find(item => item.id == id)?.quantity ?? 0;
        public int UsedCapacity => state.Data.inventory.items.Sum(item => Math.Max(0, item.quantity));
        public int Capacity => Math.Max(defaultCapacity, state.Data.inventory.capacity);
        private static List<ItemStack> Aggregate(IEnumerable<ItemStack> requirements)
        {
            if (requirements == null) return null;
            var totals = new Dictionary<string, int>();
            foreach (ItemStack item in requirements)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.id) || item.quantity <= 0) return null;
                totals.TryGetValue(item.id, out int current);
                if ((long)current + item.quantity > int.MaxValue) return null;
                totals[item.id] = current + item.quantity;
            }
            return totals.Select(pair => new ItemStack { id = pair.Key, quantity = pair.Value }).ToList();
        }
        public bool Has(IEnumerable<ItemStack> requirements)
        {
            var totals = Aggregate(requirements);
            return totals != null && totals.All(item => Amount(item.id) >= item.quantity);
        }
        public IEnumerable<ItemStack> InCategory(ItemCategory category) => state.Data.inventory.items.Where(item => GameServices.Instance?.Balance.catalog?.Item(item.id)?.category == category);

        public bool Add(string id, int quantity, bool enforceCapacity = false)
        {
            if (string.IsNullOrWhiteSpace(id) || quantity <= 0 || enforceCapacity && UsedCapacity + quantity > Capacity) return false;
            ItemStack item = state.Data.inventory.items.Find(value => value.id == id);
            if (item == null) { item = new ItemStack { id = id }; state.Data.inventory.items.Add(item); }
            item.quantity += quantity;
            Changed?.Invoke(id, item.quantity);
            state.NotifyChanged();
            return true;
        }

        public bool Remove(IReadOnlyCollection<ItemStack> requirements)
        {
            var totals = Aggregate(requirements);
            if (totals == null || !totals.All(item => Amount(item.id) >= item.quantity)) return false;
            foreach (ItemStack required in totals)
            {
                ItemStack item = state.Data.inventory.items.Find(value => value.id == required.id);
                item.quantity -= required.quantity;

            }
            foreach (ItemStack required in totals) Changed?.Invoke(required.id, Amount(required.id));
            state.NotifyChanged();
            return true;
        }
    }
}
