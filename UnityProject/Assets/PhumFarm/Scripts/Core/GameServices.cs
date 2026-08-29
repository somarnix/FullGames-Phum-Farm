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
        public TimeSystem Time { get; private set; }
        public AudioManager Audio { get; private set; }
        public TutorialSystem Tutorial { get; private set; }
        public OrderSystem Orders { get; private set; }
        public CropSystem Crops { get; private set; }
        public AnimalSystem Animals { get; private set; }
        public ProductionSystem Production { get; private set; }
        public JourneySystem Journey { get; private set; }

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
            Time = new TimeSystem(State);
            Localization = new LocalizationService(State);
            Scenes = new SceneManager();
            Saves = new SaveManager(State, null, Time);
            Audio = new AudioManager(State);
            Audio.Attach(gameObject);
            Tutorial = new TutorialSystem(State);
            Orders = new OrderSystem(State, Balance, Inventory, Economy, Progression);
            Crops = new CropSystem(State, Balance, Inventory, Economy, Progression, Unlocks, Tutorial);
            Animals = new AnimalSystem(State, Balance, Inventory, Progression, Tutorial);
            Production = new ProductionSystem(State, Balance, Inventory, Progression, Economy, Tutorial);
            Journey = new JourneySystem(State, Economy, Progression);
            Orders.EnsureOrders();
        }
    }
}
