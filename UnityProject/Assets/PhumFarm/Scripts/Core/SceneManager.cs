using System;
using System.Collections;
using UnityEngine;

namespace PhumFarm.Core
{
    public enum GameScreen { MainMenu, FarmWorld }

    public sealed class SceneManager
    {
        public GameScreen Current { get; private set; } = GameScreen.MainMenu;
        public event Action<GameScreen> Changed;

        public IEnumerator OpenFarm()
        {
            yield return Load("FarmWorld", GameScreen.FarmWorld);
        }

        public IEnumerator OpenMainMenu()
        {
            yield return Load("MainMenu", GameScreen.MainMenu);
        }

        private IEnumerator Load(string sceneName, GameScreen screen)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneName)
            {
                AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
                if (operation == null) throw new InvalidOperationException($"Scene '{sceneName}' is missing from Build Settings");
                while (!operation.isDone) yield return null;
            }
            Current = screen;
            Changed?.Invoke(Current);
        }
    }
}
