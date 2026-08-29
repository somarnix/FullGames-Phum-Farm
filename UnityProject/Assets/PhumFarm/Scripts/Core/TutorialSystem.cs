using System;

namespace PhumFarm.Core
{
    public enum TutorialAction
    {
        MoveCamera = 0,
        SelectField = 1,
        TillField = 2,
        PlantRice = 3,
        HarvestRice = 4,
        OpenInventory = 5,
        FeedChicken = 6,
        CollectEgg = 7,
        CompleteOrder = 8,
        LevelUp = 9
    }

    public sealed class TutorialSystem
    {
        public const int StepCount = 10;
        private readonly GameState state;
        public event Action<int> StepChanged;

        public TutorialSystem(GameState state) => this.state = state;
        public int CurrentStep => state.Data.player.tutorialStep;
        public bool Completed => state.Data.player.tutorialCompleted || CurrentStep >= StepCount;

        public bool Record(TutorialAction action)
        {
            if (Completed || (int)action != CurrentStep) return false;
            state.Data.player.tutorialStep++;
            state.Data.player.tutorialCompleted = state.Data.player.tutorialStep >= StepCount;
            StepChanged?.Invoke(state.Data.player.tutorialStep);
            state.NotifyChanged();
            GameServices.Instance?.Saves.Save();
            return true;
        }

        public void Skip()
        {
            state.Data.player.tutorialStep = StepCount;
            state.Data.player.tutorialCompleted = true;
            StepChanged?.Invoke(StepCount);
            state.NotifyChanged();
        }

        public void Reset()
        {
            state.Data.player.tutorialStep = 0;
            state.Data.player.tutorialCompleted = false;
            StepChanged?.Invoke(0);
            state.NotifyChanged();
        }
    }
}
