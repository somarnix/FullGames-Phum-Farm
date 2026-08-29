using System;
using System.Collections;
using System.Linq;
using PhumFarm.Core;
using PhumFarm.Data;
using PhumFarm.Gameplay;
using PhumFarm.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PhumFarm.UI
{
    public sealed partial class GameUiController : MonoBehaviour
    {
        public event Action NewFarmRequested;
        public event Action ContinueRequested;
        public event Action MainMenuRequested;
        public event Action SaveRequested;

        private Canvas canvas;
        private Font font;
        private GameObject mainMenu;
        private GameObject hud;
        private GameObject modal;
        private Text titleText;
        private Text subtitleText;
        private Text languageText;
        private Text newFarmText;
        private Text continueText;
        private Button continueButton;
        private Text statusText;
        private Text currencyText;
        private Text clockText;
        private Text questText;
        private Text promptText;
        private Text toastText;
        private Coroutine toastRoutine;
        private FarmWorldController world;
        private FarmPlot selectedPlot;
        private float nextHudRefresh;

        private readonly Color panelColor = new(.08f, .12f, .09f, .94f);
        private readonly Color green = new(.21f, .52f, .29f, 1f);
        private readonly Color gold = new(.91f, .64f, .18f, 1f);
        private readonly Color cream = new(.99f, .95f, .82f, 1f);
        private readonly Color wood = new(.27f, .14f, .08f, .96f);
        private readonly Color terracotta = new(.78f, .36f, .20f, 1f);

        public static GameUiController Create(Transform parent)
        {
            var root = new GameObject("GameUI");
            root.transform.SetParent(parent, false);
            return root.AddComponent<GameUiController>();
        }

        private void Awake()
        {
            font = Resources.Load<Font>("Fonts/PhumFarmFont") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvas();
            BuildMainMenu();
            BuildHud();
            GameServices services = GameServices.Instance;
            services.State.Changed += Refresh;
            services.State.MessageRequested += ShowToast;
            services.State.LevelUp += ShowLevelUp;
            services.Localization.Changed += RefreshLanguage;
        }

        public void Bind(FarmWorldController value)
        {
            world = value;
            world.CropSelectionRequested += ShowCropSelector;
            world.StationContextRequested += ShowStationScreen;
            world.Player.PromptChanged += SetPrompt;
            Refresh();
        }

        public void ShowMainMenu(bool hasSave)
        {
            CloseModal();
            mainMenu.SetActive(true);
            hud.SetActive(false);
            continueButton.interactable = hasSave;
            RefreshLanguage();
        }

        public void ShowGameplay()
        {
            mainMenu.SetActive(false);
            hud.SetActive(true);
            Refresh();
            SetPrompt(GameServices.Instance.Localization.Get("prompt.move"));
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            if (EventSystem.current != null) return;
            GameObject eventObject = new("EventSystem");
            eventObject.transform.SetParent(transform, false);
            eventObject.AddComponent<EventSystem>();
            eventObject.AddComponent<StandaloneInputModule>();
        }

        private void BuildMainMenu()
        {
            mainMenu = Panel(canvas.transform, "MainMenu", new Color(.08f, .035f, .015f, .36f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Panel(mainMenu.transform, "TopVignette", new Color(.12f, .045f, .015f, .55f), new Vector2(0, .78f), Vector2.one, Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
            Panel(mainMenu.transform, "BottomVignette", new Color(.08f, .025f, .01f, .68f), Vector2.zero, new Vector2(1, .28f), Vector2.zero, Vector2.zero).GetComponent<Image>().raycastTarget = false;
            GameObject card = Panel(mainMenu.transform, "MenuCard", wood, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-390, -355), new Vector2(390, 355));
            Panel(card.transform, "Accent", gold, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -12), Vector2.zero).GetComponent<Image>().raycastTarget = false;
            titleText = Label(card.transform, "Title", "PHUM FARM", 78, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one, new Vector2(25, -165), new Vector2(-25, -40), WorldPrimitiveFactory.Hex("FFF1C1"));
            subtitleText = Label(card.transform, "Subtitle", string.Empty, 24, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one, new Vector2(30, -225), new Vector2(-30, -160), gold);
            Label(card.transform, "Promise", "Grow your village. Share the harvest.", 20, FontStyle.Italic, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one, new Vector2(30, -275), new Vector2(-30, -220), cream);
            Button newButton = MakeButton(card.transform, "NewFarm", string.Empty, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-255, -375), new Vector2(255, -292), green, () => NewFarmRequested?.Invoke());
            newFarmText = newButton.GetComponentInChildren<Text>();
            continueButton = MakeButton(card.transform, "Continue", string.Empty, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-255, -472), new Vector2(255, -395), WorldPrimitiveFactory.Hex("3D7048"), () => ContinueRequested?.Invoke());
            continueText = continueButton.GetComponentInChildren<Text>();
            MakeButton(card.transform, "Profile", "PROFILE", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-275, 90), new Vector2(-95, 155), WorldPrimitiveFactory.Hex("75533A"), ShowProfile);
            Button languageButton = MakeButton(card.transform, "Language", string.Empty, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-85, 90), new Vector2(95, 155), WorldPrimitiveFactory.Hex("44584B"), () => GameServices.Instance.Localization.Cycle());
            languageText = languageButton.GetComponentInChildren<Text>();
            MakeButton(card.transform, "Settings", "SETTINGS", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(105, 90), new Vector2(285, 155), WorldPrimitiveFactory.Hex("75533A"), ShowSettings);
            Label(card.transform, "Version", "v1.0  •  Unity Edition", 17, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1, 0), new Vector2(20, 22), new Vector2(-20, 68), new Color(1, 1, 1, .62f));
        }

        private void BuildHud()
        {
            hud = new GameObject("GameplayHUD", typeof(RectTransform));
            hud.transform.SetParent(canvas.transform, false);
            Stretch(hud.GetComponent<RectTransform>());

            GameObject status = Panel(hud.transform, "PlayerStatus", wood, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -112), new Vector2(390, -20));
            Label(status.transform, "Portrait", "PF", 30, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(0, 1), new Vector2(12, 12), new Vector2(86, -12), gold);
            statusText = Label(status.transform, "Status", string.Empty, 22, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(102, 10), new Vector2(-18, -10), cream);

            GameObject time = Panel(hud.transform, "Time", wood, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-150, -90), new Vector2(150, -20));
            clockText = Label(time.transform, "Clock", string.Empty, 23, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(12, 6), new Vector2(-12, -6), cream);

            GameObject currency = Panel(hud.transform, "Currency", wood, Vector2.one, Vector2.one, new Vector2(-455, -90), new Vector2(-95, -20));
            currencyText = Label(currency.transform, "CurrencyText", string.Empty, 22, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(12, 6), new Vector2(-12, -6), cream);
            MakeButton(hud.transform, "Pause", "MENU", Vector2.one, Vector2.one, new Vector2(-180, -168), new Vector2(-20, -105), WorldPrimitiveFactory.Hex("704A42"), ShowPauseMenu);

            GameObject quest = Panel(hud.transform, "Quest", cream, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -260), new Vector2(370, -132));
            questText = Label(quest.transform, "QuestText", string.Empty, 19, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.one, new Vector2(22, 12), new Vector2(-46, -12), WorldPrimitiveFactory.Hex("493226"));
            quest.AddComponent<Button>().onClick.AddListener(ShowQuestBook);
            Label(quest.transform, "Arrow", ">", 28, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(1, 0), Vector2.one, new Vector2(-44, 0), Vector2.zero, terracotta);

            GameObject prompt = Panel(hud.transform, "Prompt", wood, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-390, 114), new Vector2(390, 174));
            promptText = Label(prompt.transform, "Text", string.Empty, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(18, 5), new Vector2(-18, -5), cream);

            GameObject nav = Panel(hud.transform, "BottomNavigation", wood, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-500, 18), new Vector2(500, 104));
            string[] names = { "SHOP", "BUILD", "ORDERS", "BAG", "MAP" };
            Action[] actions = { ShowShop, ShowBuildMode, ShowOrders, ShowInventory, ShowMap };
            for (int i = 0; i < names.Length; i++)
            {
                int captured = i;
                MakeButton(nav.transform, names[i], names[i], Vector2.zero, new Vector2(0, 1), new Vector2(12 + i * 196, 10), new Vector2(190 + i * 196, -10), i == 2 ? terracotta : green, () => actions[captured]());
            }

            toastText = Label(hud.transform, "Toast", string.Empty, 25, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-390, -195), new Vector2(390, -128), WorldPrimitiveFactory.Hex("FFF2B3"));
            toastText.gameObject.SetActive(false);
            hud.SetActive(false);
        }

        private void Refresh()
        {
            if (statusText == null) return;
            GameServices services = GameServices.Instance;
            GameSaveData data = services.State.Data;
            LocalizationService loc = services.Localization;
            statusText.text = $"{loc.Get("hud.level")} {data.player.level}\nXP {data.player.xp}/{services.Progression.NextLevelXp}";
            currencyText.text = $"● {data.economy.coins:N0} coins     ◆ {data.economy.gems} gems";
            clockText.text = $"☀  {loc.Get("hud.day")} {data.world.day}   {services.State.ClockText()}";
            string task = data.player.tutorialStep switch
            {
                0 => "1. Till a field",
                1 => "2. Plant your first crop",
                2 => "3. Harvest when it is ready",
                3 => "4. Sell goods at the village market",
                _ => "Grow the village and unlock districts"
            };
            questText.text = $"{loc.Get("hud.quest")}\n{task}\nHarvested: {data.player.totalHarvested}";
        }

        private void RefreshLanguage()
        {
            LocalizationService loc = GameServices.Instance.Localization;
            font = LanguageFont(loc.Language) ?? font;
            foreach (Text label in canvas.GetComponentsInChildren<Text>(true)) label.font = font;
            titleText.text = loc.Get("game.title");
            subtitleText.text = loc.Get("game.subtitle");
            newFarmText.text = loc.Get("menu.new");
            continueText.text = loc.Get("menu.continue");
            languageText.text = loc.Get("language");
            Refresh();
        }

        private static Font LanguageFont(GameLanguage language)
        {
            string[] names = language switch
            {
                GameLanguage.Khmer => new[] { "Khmer UI", "Nirmala UI", "Arial Unicode MS", "Arial" },
                GameLanguage.Chinese => new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Arial Unicode MS", "Arial" },
                _ => new[] { "Arial", "Arial Unicode MS" }
            };
            return Font.CreateDynamicFontFromOSFont(names, 26);
        }

        private void ShowCropSelector(FarmPlot plot)
        {
            selectedPlot = plot;
            GameObject card = OpenModal(GameServices.Instance.Localization.Get("crop.choose"), 720, 650);
            float y = -115f;
            foreach (CropDefinition crop in GameServices.Instance.Balance.crops)
            {
                CropDefinition captured = crop;
                bool unlocked = GameServices.Instance.Unlocks.Crop(crop.id);
                string text = unlocked ? $"{TitleCase(crop.id)}   •   {crop.seedCost} coins   •   {crop.growSeconds:0}s" : $"{TitleCase(crop.id)}   •   Unlocks at level {crop.unlockLevel}";
                Button button = MakeButton(card.transform, crop.id, text, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(-285, y - 68), new Vector2(285, y), unlocked ? green : WorldPrimitiveFactory.Hex("4B514D"), () => Plant(captured.id));
                button.interactable = unlocked;
                y -= 84f;
            }
        }

        private void Plant(string cropId)
        {
            if (selectedPlot != null && selectedPlot.Plant(cropId)) CloseModal();
        }

        private GameObject OpenModal(string title, float width, float height)
        {
            CloseModal();
            modal = Panel(canvas.transform, "ModalShade", new Color(0, 0, 0, .68f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            GameObject card = Panel(modal.transform, "Card", wood, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(-width / 2, -height / 2), new Vector2(width / 2, height / 2));
            Panel(card.transform, "HeaderAccent", gold, new Vector2(0, 1), Vector2.one, new Vector2(0, -8), Vector2.zero).GetComponent<Image>().raycastTarget = false;
            Label(card.transform, "Title", title, 36, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, 1), Vector2.one, new Vector2(25, -80), new Vector2(-25, -15), WorldPrimitiveFactory.Hex("F3D77B"));
            MakeButton(card.transform, "Close", "X", Vector2.one, Vector2.one, new Vector2(-72, -72), new Vector2(-14, -14), WorldPrimitiveFactory.Hex("704A42"), CloseModal);
            return card;
        }

        private void CloseModal()
        {
            if (modal != null) Destroy(modal);
            modal = null;
            selectedPlot = null;
        }

        private void SetPrompt(string value)
        {
            if (promptText != null && !string.IsNullOrWhiteSpace(value)) promptText.text = value;
        }

        private void ShowToast(string message)
        {
            if (toastRoutine != null) StopCoroutine(toastRoutine);
            toastRoutine = StartCoroutine(Toast(message));
        }

        private IEnumerator Toast(string message)
        {
            toastText.text = message;
            toastText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2.5f);
            toastText.gameObject.SetActive(false);
            toastRoutine = null;
        }

        private void ShowLevelUp(int level) => ShowLevelUpScreen(level);

        private void Update()
        {
            if (hud == null || !hud.activeSelf || Time.unscaledTime < nextHudRefresh) return;
            nextHudRefresh = Time.unscaledTime + .5f;
            Refresh();
        }

        private GameObject Panel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject value = new(name, typeof(RectTransform), typeof(Image));
            value.transform.SetParent(parent, false);
            RectTransform rect = value.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            value.GetComponent<Image>().color = color;
            return value;
        }

        private Text Label(Transform parent, string name, string value, int size, FontStyle style, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            GameObject node = new(name, typeof(RectTransform), typeof(Text));
            node.transform.SetParent(parent, false);
            RectTransform rect = node.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            Text label = node.GetComponent<Text>();
            label.font = font; label.text = value; label.fontSize = size; label.fontStyle = style; label.alignment = alignment; label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        private Button MakeButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color, Action action)
        {
            GameObject node = Panel(parent, name, color, anchorMin, anchorMax, offsetMin, offsetMax);
            Button button = node.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, .15f);
            colors.pressedColor = Color.Lerp(color, Color.black, .2f);
            colors.disabledColor = new Color(.25f, .27f, .25f, .72f);
            button.colors = colors;
            button.onClick.AddListener(() => action?.Invoke());
            Label(node.transform, "Text", text, 21, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8, 4), new Vector2(-8, -4), Color.white);
            return button;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        }

        private static string TitleCase(string id) => string.Join(" ", id.Split('_').Select(value => char.ToUpperInvariant(value[0]) + value.Substring(1)));

        private void OnDestroy()
        {
            if (GameServices.Instance == null) return;
            GameServices.Instance.State.Changed -= Refresh;
            GameServices.Instance.State.MessageRequested -= ShowToast;
            GameServices.Instance.State.LevelUp -= ShowLevelUp;
            GameServices.Instance.Localization.Changed -= RefreshLanguage;
        }
    }
}
