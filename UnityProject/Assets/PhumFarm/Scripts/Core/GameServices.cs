using PhumFarm.Data;
using UnityEngine;

namespace PhumFarm.Core
{
    public sealed class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }
        public GameBalance Balance { get; private set; }
        public GameState State { get; private set; }
        public SaveManager Saves { get; private set; }
        public InventorySystem Inventory { get; private set; }
        public EconomySystem Economy { get; private set; }
        public ProgressionSystem Progression { get; private set; }
        public UnlockSystem Unlocks { get; private set; }
        public LocalizationService Localization { get; private set; }
        public SceneManager Scenes { get; private set; }

        public static GameServices Ensure()
        {
            if (Instance != null) return Instance;
            var root = new GameObject("GameServices");
            return root.AddComponent<GameServices>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Balance = GameBalance.Load();
            State = new GameState(Balance);
            Inventory = new InventorySystem(State, Balance.barnCapacity);
            Economy = new EconomySystem(State);
            Progression = new ProgressionSystem(State, Balance);
            Unlocks = new UnlockSystem(State, Balance);
            Localization = new LocalizationService();
            Scenes = new SceneManager();
            Saves = new SaveManager(State);
        }
    }
}
