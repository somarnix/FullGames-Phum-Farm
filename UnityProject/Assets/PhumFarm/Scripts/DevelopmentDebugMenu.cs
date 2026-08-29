#if UNITY_EDITOR || DEVELOPMENT_BUILD
using PhumFarm.Core;
using UnityEngine;

namespace PhumFarm
{
    public sealed class DevelopmentDebugMenu : MonoBehaviour
    {
        private bool visible;
        private Rect window = new(30, 30, 330, 380);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10)) visible = !visible;
        }

        private void OnGUI()
        {
            if (!visible) return;
            window = GUI.Window(93841, window, DrawWindow, "Phum Farm — Development Only");
        }

        private void DrawWindow(int id)
        {
            GameServices services = GameServices.Instance;
            GUILayout.Label($"Level {services.State.Data.player.level} • XP {services.State.Data.player.xp}");
            GUILayout.Label($"Coins {services.Economy.Coins} • Gems {services.Economy.Gems}");
            GUILayout.Space(10);
            if (GUILayout.Button("Add 1,000 coins")) services.Economy.AddCoins(1000);
            if (GUILayout.Button("Add 250 XP")) services.Progression.AddXp(250);
            if (GUILayout.Button("Advance 6 hours")) services.State.AdvanceTime(120f);
            if (GUILayout.Button("Fill basic materials"))
            {
                foreach (string item in new[] { "rice", "corn", "tomato", "sugarcane", "animal_feed", "milk" }) services.Inventory.Add(item, 10);
            }
            if (GUILayout.Button("Save now")) services.Saves.Save();
            GUILayout.Space(14);
            GUI.color = new Color(1f, .55f, .45f);
            if (GUILayout.Button("Reset current farm")) services.State.NewGame();
            GUI.color = Color.white;
            GUILayout.Label("F10 toggles this menu. This code is excluded from non-development builds.");
            GUI.DragWindow(new Rect(0, 0, 10000, 25));
        }
    }
}
#endif
