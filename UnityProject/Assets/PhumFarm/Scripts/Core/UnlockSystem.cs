using PhumFarm.Data;

namespace PhumFarm.Core
{
    public sealed class UnlockSystem
    {
        private readonly GameState state;
        private readonly GameBalance balance;
        public UnlockSystem(GameState state, GameBalance balance) { this.state = state; this.balance = balance; }
        public bool Level(int required) => state.Data.player.level >= required;
        public bool Crop(string id) => balance.Crop(id) is { } value && Level(value.unlockLevel);
        public bool Animal(string id) => balance.Animal(id) is { } value && Level(value.unlockLevel);
        public bool Plot(int index) => index >= 0 && index < balance.plotUnlockLevels.Length && Level(balance.plotUnlockLevels[index]);
        public bool Expansion(string id) => state.Data.world.expansions.Find(value => value.id == id)?.unlocked ?? false;
        public bool CanBuyExpansion(string id)
        {
            ExpansionDefinition value = balance.Expansion(id);
            return value != null && !Expansion(id) && Level(value.requiredLevel) && state.Data.economy.coins >= value.coinCost;
        }
    }
}
