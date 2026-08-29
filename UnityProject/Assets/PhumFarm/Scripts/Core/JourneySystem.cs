using System;
using System.Collections.Generic;

namespace PhumFarm.Core
{
    [Serializable] public sealed class JourneyState
    {
        public long dayKey = -1;
        public int dailyHarvest, dailyOrders, dailyProducts;
        public int totalOrders, totalProducts;
        public List<string> dailyClaims = new();
        public List<string> achievementClaims = new();
    }

    public sealed class JourneyGoal
    {
        public string Id, Title;
        public int Progress, Target, Coins, Xp;
        public bool Claimed;
        public bool Ready => !Claimed && Progress >= Target;
    }

    public sealed class JourneySystem
    {
        private readonly GameState state;
        private readonly EconomySystem economy;
        private readonly ProgressionSystem progression;
        private readonly Func<long> utcDay;
        public JourneySystem(GameState state, EconomySystem economy, ProgressionSystem progression, Func<long> utcDay = null)
        {
            this.state = state; this.economy = economy; this.progression = progression;
            this.utcDay = utcDay ?? (() => DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 86400);
        }
        private JourneyState Current()
        {
            JourneyState value = state.Data.journey ??= new JourneyState();
            value.dailyClaims ??= new List<string>(); value.achievementClaims ??= new List<string>();
            long today = utcDay();
            // A clock rollback must not reopen a previously claimed day.
            if (today > value.dayKey)
            {
                value.dayKey = today;
                value.dailyHarvest = value.dailyOrders = value.dailyProducts = 0;
                value.dailyClaims.Clear();
            }
            return value;
        }
        public void RecordHarvest(int amount) { if (amount <= 0) return; Current().dailyHarvest += amount; state.NotifyChanged(); }
        public void RecordOrder() { var value = Current(); value.dailyOrders++; value.totalOrders++; state.NotifyChanged(); }
        public void RecordProduct() { var value = Current(); value.dailyProducts++; value.totalProducts++; state.NotifyChanged(); }
        public JourneyGoal[] Goals(bool daily)
        {
            var value = Current();
            return daily ? new[] {
                Goal("harvest", "Harvest 15 crop items", value.dailyHarvest, 15, 40, 10, value.dailyClaims),
                Goal("orders", "Deliver 3 village orders", value.dailyOrders, 3, 60, 15, value.dailyClaims),
                Goal("products", "Collect 3 crafted products", value.dailyProducts, 3, 50, 15, value.dailyClaims)
            } : new[] {
                Goal("harvester", "Harvest 100 crop items", state.Data.player.totalHarvested, 100, 150, 30, value.achievementClaims),
                Goal("neighbor", "Deliver 25 village orders", value.totalOrders, 25, 200, 50, value.achievementClaims),
                Goal("artisan", "Collect 50 crafted products", value.totalProducts, 50, 250, 60, value.achievementClaims)
            };
        }
        private static JourneyGoal Goal(string id, string title, int progress, int target, int coins, int xp, List<string> claims)
            => new JourneyGoal { Id = id, Title = title, Progress = progress, Target = target, Coins = coins, Xp = xp, Claimed = claims.Contains(id) };
        public bool Claim(string id, bool daily)
        {
            JourneyGoal goal = Array.Find(Goals(daily), item => item.Id == id);
            if (goal == null || !goal.Ready) return false;
            var value = Current();
            (daily ? value.dailyClaims : value.achievementClaims).Add(id);
            economy.AddCoins(goal.Coins); progression.AddXp(goal.Xp); state.NotifyChanged();
            return true;
        }
    }
}
