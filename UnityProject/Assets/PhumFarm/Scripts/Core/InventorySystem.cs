using System;
using System.Collections.Generic;
using System.Linq;

namespace PhumFarm.Core
{
    public sealed class InventorySystem
    {
        private readonly GameState state;
        private readonly int capacity;
        public event Action<string, int> Changed;

        public InventorySystem(GameState state, int capacity) { this.state = state; this.capacity = Math.Max(1, capacity); }
        public int Amount(string id) => state.Data.inventory.items.Find(item => item.id == id)?.quantity ?? 0;
        public int UsedCapacity => state.Data.inventory.items.Sum(item => Math.Max(0, item.quantity));
        public int Capacity => capacity;
        public bool Has(IEnumerable<ItemStack> requirements) => requirements.All(item => Amount(item.id) >= item.quantity);

        public bool Add(string id, int quantity, bool enforceCapacity = false)
        {
            if (string.IsNullOrWhiteSpace(id) || quantity <= 0 || enforceCapacity && UsedCapacity + quantity > capacity) return false;
            ItemStack item = state.Data.inventory.items.Find(value => value.id == id);
            if (item == null) { item = new ItemStack { id = id }; state.Data.inventory.items.Add(item); }
            item.quantity += quantity;
            Changed?.Invoke(id, item.quantity);
            state.NotifyChanged();
            return true;
        }

        public bool Remove(IReadOnlyCollection<ItemStack> requirements)
        {
            if (!Has(requirements)) return false;
            foreach (ItemStack required in requirements)
            {
                ItemStack item = state.Data.inventory.items.Find(value => value.id == required.id);
                item.quantity -= required.quantity;
                Changed?.Invoke(item.id, item.quantity);
            }
            state.NotifyChanged();
            return true;
        }
    }
}
