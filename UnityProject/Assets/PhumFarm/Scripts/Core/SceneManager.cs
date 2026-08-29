using System;

namespace PhumFarm.Core
{
    public enum GameScreen { MainMenu, FarmWorld }

    public sealed class SceneManager
    {
        public GameScreen Current { get; private set; } = GameScreen.MainMenu;
        public event Action<GameScreen> Changed;

        public void OpenFarm()
        {
            Current = GameScreen.FarmWorld;
            Changed?.Invoke(Current);
        }

        public void OpenMainMenu()
        {
            Current = GameScreen.MainMenu;
            Changed?.Invoke(Current);
        }
    }
}
