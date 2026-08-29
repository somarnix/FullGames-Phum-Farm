using System.IO;
using NUnit.Framework;
using PhumFarm.Core;
using PhumFarm.Data;

namespace PhumFarm.Tests
{
    public sealed class CoreSystemsTests
    {
        private GameBalance balance;
        private GameState state;

        [SetUp]
        public void SetUp()
        {
            balance = GameBalance.Load();
            state = new GameState(balance);
        }

        [Test]
        public void NewFarmHasExpectedStartingState()
        {
            Assert.AreEqual(1, state.Data.player.level);
            Assert.AreEqual(120, state.Data.economy.coins);
            Assert.AreEqual(16, state.Data.crops.plots.Count);
            Assert.AreEqual(4, state.Data.inventory.items.Find(value => value.id == "rice").quantity);
        }

        [Test]
        public void InventoryAndEconomyTransactionsAreAtomic()
        {
            var inventory = new InventorySystem(state, balance.barnCapacity);
            var economy = new EconomySystem(state);
            Assert.IsTrue(economy.SpendCoins(20));
            Assert.AreEqual(100, economy.Coins);
            Assert.IsFalse(economy.SpendCoins(101));
            Assert.IsTrue(inventory.Add("rice", 3));
            Assert.AreEqual(7, inventory.Amount("rice"));
        }

        [Test]
        public void ProgressionUnlocksContentByLevel()
        {
            var progression = new ProgressionSystem(state, balance);
            var unlocks = new UnlockSystem(state, balance);
            Assert.IsFalse(unlocks.Crop("corn"));
            progression.AddXp(45);
            Assert.AreEqual(2, progression.CurrentLevel);
            Assert.IsTrue(unlocks.Crop("corn"));
        }

        [Test]
        public void SaveRoundTripPreservesFarmData()
        {
            string path = Path.Combine(Path.GetTempPath(), $"phum_farm_test_{System.Guid.NewGuid():N}.json");
            var saves = new SaveManager(state, path);
            try
            {
                state.Data.economy.coins = 777;
                Assert.IsTrue(saves.Save(), saves.LastError);
                state.Data.economy.coins = 1;
                Assert.IsTrue(saves.Load(), saves.LastError);
                Assert.AreEqual(777, state.Data.economy.coins);
                Assert.AreEqual(SaveManager.CurrentSaveVersion, state.Data.saveVersion);
            }
            finally
            {
                saves.Delete();
            }
        }

        [Test]
        public void LocalizationCyclesEnglishKhmerAndChineseWithoutCorruptText()
        {
            var localization = new LocalizationService();
            Assert.AreEqual("PHUM FARM", localization.Get("game.title"));
            localization.Cycle();
            Assert.AreEqual("ភូមិ ហ្វាម", localization.Get("game.title"));
            localization.Cycle();
            Assert.AreEqual("村庄农场", localization.Get("game.title"));
        }
    }
}
