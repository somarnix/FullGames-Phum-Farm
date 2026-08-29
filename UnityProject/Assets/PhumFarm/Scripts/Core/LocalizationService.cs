using System;
using System.Collections.Generic;

namespace PhumFarm.Core
{
    public enum GameLanguage { English, Khmer, Chinese }

    public sealed class LocalizationService
    {
        private readonly GameState state;
        public GameLanguage Language => (GameLanguage)Math.Max(0, Math.Min(2, state.Data.settings.language));
        public event Action Changed;

        private static readonly Dictionary<string, string[]> Strings = new()
        {
            ["game.title"] = new[] { "PHUM FARM", "ភូមិ ហ្វាម", "村庄农场" },
            ["game.subtitle"] = new[] { "A Cambodian village farming adventure", "ដំណើរផ្សងព្រេងកសិកម្មនៅភូមិខ្មែរ", "柬埔寨村庄农场冒险" },
            ["menu.new"] = new[] { "New Farm", "កសិដ្ឋានថ្មី", "新农场" },
            ["menu.continue"] = new[] { "Continue", "បន្ត", "继续" },
            ["menu.save"] = new[] { "Save Farm", "រក្សាទុក", "保存农场" },
            ["menu.inventory"] = new[] { "Barn", "ជង្រុក", "谷仓" },
            ["menu.main"] = new[] { "Main Menu", "ម៉ឺនុយដើម", "主菜单" },
            ["menu.close"] = new[] { "Close", "បិទ", "关闭" },
            ["menu.settings"] = new[] { "Settings", "ការកំណត់", "设置" },
            ["hud.level"] = new[] { "Level", "កម្រិត", "等级" },
            ["hud.coins"] = new[] { "Coins", "កាក់", "金币" },
            ["hud.gems"] = new[] { "Gems", "ត្បូង", "宝石" },
            ["hud.day"] = new[] { "Day", "ថ្ងៃ", "天" },
            ["hud.quest"] = new[] { "Farm Journey", "ដំណើរកសិដ្ឋាន", "农场旅程" },
            ["prompt.move"] = new[] { "Move to an activity and interact", "ទៅកាន់សកម្មភាព ហើយធ្វើអន្តរកម្ម", "移动到活动处并互动" },
            ["crop.choose"] = new[] { "Choose a crop", "ជ្រើសរើសដំណាំ", "选择作物" },
            ["inventory.empty"] = new[] { "Your barn is empty.", "ជង្រុករបស់អ្នកទទេ។", "你的谷仓是空的。" },
            ["language"] = new[] { "Language: English", "ភាសា៖ ខ្មែរ", "语言：中文" }
        };

        public LocalizationService(GameState state) => this.state = state;
        public string Get(string key) => Strings.TryGetValue(key, out string[] values) ? values[(int)Language] : key;

        public void Set(GameLanguage language)
        {
            state.Data.settings.language = (int)language;
            Changed?.Invoke();
            state.NotifyChanged();
            GameServices.Instance?.Saves.Save();
        }

        public void Cycle() => Set((GameLanguage)(((int)Language + 1) % 3));
    }
}
