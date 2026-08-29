using PhumFarm.Core;
using PhumFarm.UI;
using PhumFarm.World;
using UnityEngine;

namespace PhumFarm
{
    public sealed class GameFlowController : MonoBehaviour
    {
        public FarmWorldController World { get; private set; }
        private GameUiController ui;

        private void Start()
        {
            ui = GameUiController.Create(transform);
            ui.NewFarmRequested += StartNewFarm;
            ui.ContinueRequested += ContinueFarm;
            ui.MainMenuRequested += ShowMainMenu;
            ui.SaveRequested += Save;
            ShowMainMenu();
        }

        private void StartNewFarm()
        {
            GameServices.Instance.State.NewGame();
            OpenWorld();
        }

        private void ContinueFarm()
        {
            if (!GameServices.Instance.Saves.Load()) GameServices.Instance.State.NewGame();
            OpenWorld();
        }

        private void OpenWorld()
        {
            GameServices.Instance.Scenes.OpenFarm();
            if (World != null) Destroy(World.gameObject);
            World = FarmWorldController.Create();
            World.name = "PlayableFarmWorld";
            ui.Bind(World);
            ui.ShowGameplay();
        }

        private void ShowMainMenu()
        {
            GameServices.Instance.Scenes.OpenMainMenu();
            if (World != null) Destroy(World.gameObject);
            World = FarmWorldController.Create();
            World.name = "MainMenuFarmWorld";
            World.enabled = false;
            World.Player.enabled = false;
            ui.ShowMainMenu(GameServices.Instance.Saves.HasSave);
        }

        private void Save() => GameServices.Instance.Saves.Save();

        private void OnApplicationPause(bool paused)
        {
            if (paused && World != null && World.enabled) GameServices.Instance.Saves.Save();
        }

        private void OnApplicationQuit()
        {
            if (World != null && World.enabled) GameServices.Instance.Saves.Save();
        }
    }
}
