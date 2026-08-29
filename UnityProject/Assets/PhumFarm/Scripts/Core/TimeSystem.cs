using System;

namespace PhumFarm.Core
{
    public sealed class TimeSystem
    {
        public const double GameMinutesPerRealSecond = 3d;
        public const double MaximumOfflineSeconds = 7d * 24d * 60d * 60d;

        private readonly GameState state;
        public double LastOfflineSeconds { get; private set; }

        public TimeSystem(GameState state) => this.state = state;

        public void Tick(float realSeconds) => state.AdvanceTime(realSeconds);

        public double ApplyOfflineProgress(long savedAtUnix, long? nowUnix = null)
        {
            if (savedAtUnix <= 0) return 0d;
            long current = nowUnix ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            LastOfflineSeconds = Math.Min(MaximumOfflineSeconds, Math.Max(0d, current - savedAtUnix));
            state.AdvanceGameMinutes(LastOfflineSeconds * GameMinutesPerRealSecond);
            return LastOfflineSeconds;
        }

        public double Remaining(double readyAt) => Math.Max(0d, readyAt - state.GameMinutes);
        public bool IsReady(double readyAt) => readyAt > 0d && state.GameMinutes >= readyAt;
    }
}
