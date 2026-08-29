using System;

namespace PhumFarm.Core
{
    public sealed class EconomySystem
    {
        private readonly GameState state;
        public event Action<int, int> CoinsChanged;
        public event Action<int, int> GemsChanged;
        public EconomySystem(GameState state) => this.state = state;
        public int Coins => state.Data.economy.coins;
        public int Gems => state.Data.economy.gems;
        public bool CanAffordCoins(int amount) => amount >= 0 && Coins >= amount;

        public bool SpendCoins(int amount)
        {
            if (!CanAffordCoins(amount)) { state.NotifyMessage("Not enough coins"); return false; }
            state.Data.economy.coins -= amount;
            CoinsChanged?.Invoke(Coins, -amount);
            state.NotifyChanged();
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            state.Data.economy.coins += amount;
            state.Data.economy.totalEarned += amount;
            CoinsChanged?.Invoke(Coins, amount);
            state.NotifyChanged();
        }

        public bool SpendGems(int amount)
        {
            if (amount < 0 || Gems < amount) return false;
            state.Data.economy.gems -= amount;
            GemsChanged?.Invoke(Gems, -amount);
            state.NotifyChanged();
            return true;
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            state.Data.economy.gems += amount;
            GemsChanged?.Invoke(Gems, amount);
            state.NotifyChanged();
        }
    }
}
