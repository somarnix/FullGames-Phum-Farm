using System;
using System.Collections.Generic;

namespace PhumFarm.Core
{
    public enum GameLanguage { English, Khmer, Chinese }

    public sealed class LocalizationService
    {
        public GameLanguage Language { get; private set; } = GameLanguage.English;
        public event Action Changed;

        private static readonly Dictionary<string, string[]> Strings = new()
        {
            ["game.title"] = new[] { "PHUM FARM", "ភូមិ ហ្វាម", "村庄农场" },
            ["game.subtitle"] = new[] { "A Cambodian village farming adventure", "ដំណើរផ្សងព្រេងកសិកម្មនៅភូមិខ្មែរ", "柬埔寨村庄农场冒险" },
            ["menu.new"] = new[] { "New Farm", "កសិដ្ឋានថ្មី", "新农场" },
            ["menu.continue"] = new[] { "Continue", "បន្ត", "继续" },
            ["menu.save"] = new[] { "Save Farm", "រក្សាទុកកសិដ្ឋាន", "保存农场" },
            ["menu.inventory"] = new[] { "Barn", "ជង្រុក", "谷仓" },
            ["menu.main"] = new[] { "Main Menu", "ម៉ឺនុយដើម", "主菜单" },
            ["menu.close"] = new[] { "Close", "បិទ", "关闭" },
            ["hud.level"] = new[] { "Level", "កម្រិត", "等级" },
            ["hud.coins"] = new[] { "Coins", "កាក់", "金币" },
            ["hud.gems"] = new[] { "Gems", "ត្បូង", "宝石" },
            ["hud.day"] = new[] { "Day", "ថ្ងៃ", "天" },
            ["hud.quest"] = new[] { "Farm Journey", "ដំណើរកសិដ្ឋាន", "农场旅程" },
            ["prompt.move"] = new[]
            {
                "WASD or click to move  |  E to interact",
                "ប្រើ WASD ឬចុចដើម្បីដើរ  |  E ដើម្បីប្រើ",
                "使用 WASD 或点击移动  |  E 互动"
            },
            ["crop.choose"] = new[] { "Choose a crop", "ជ្រើសរើសដំណាំ", "选择作物" },
            ["inventory.empty"] = new[] { "Your barn is empty.", "ជង្រុករបស់អ្នកទទេ។", "你的谷仓是空的。" },
            ["language"] = new[] { "Language: English", "ភាសា៖ ខ្មែរ", "语言：中文" }
        };

        public string Get(string key)
        {
            if (!Strings.TryGetValue(key, out string[] values)) return key;
            return values[(int)Language];
        }

        public void Cycle()
        {
            Language = (GameLanguage)(((int)Language + 1) % 3);
            Changed?.Invoke();
        }
    }
}
