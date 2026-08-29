using System;
using PhumFarm.Data;

namespace PhumFarm.Core
{
    public sealed class ProgressionSystem
    {
        private readonly GameState state;
        private readonly GameBalance balance;
        public event Action<int, int> LevelChanged;
        public ProgressionSystem(GameState state, GameBalance balance) { this.state = state; this.balance = balance; }
        public int CurrentLevel => state.Data.player.level;
        public int CurrentXp => state.Data.player.xp;

        public int LevelForXp(int xp)
        {
            int result = 1;
            for (int index = 0; index < balance.levelThresholds.Length; index++) if (xp >= balance.levelThresholds[index]) result = index + 1;
            return result;
        }

        public int NextLevelXp => CurrentLevel >= balance.levelThresholds.Length ? balance.levelThresholds[balance.levelThresholds.Length - 1] : balance.levelThresholds[CurrentLevel];

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            int previous = CurrentLevel;
            state.Data.player.xp += amount;
            state.Data.player.level = LevelForXp(CurrentXp);
            if (CurrentLevel > previous)
            {
                for (int level = previous + 1; level <= CurrentLevel; level++)
                {
                    LevelChanged?.Invoke(level - 1, level);
                    LevelDefinitionAsset definition = balance.catalog?.levels.Find(value => value != null && value.level == level);
                    if (definition != null && definition.coinReward > 0) GameServices.Instance?.Economy.AddCoins(definition.coinReward);
                    GameServices.Instance?.Tutorial.Record(TutorialAction.LevelUp);
                    state.NotifyLevelUp(level);
                }
            }
            state.NotifyChanged();
        }
    }
}
