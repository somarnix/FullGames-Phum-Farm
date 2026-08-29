using System.Collections;
using NUnit.Framework;
using PhumFarm.Core;
using PhumFarm.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PhumFarm.Tests
{
    public sealed class GameStartupTests
    {
        [UnityTest]
        public IEnumerator BootCreatesCompleteFarmAndAllPrimaryScreensOpen()
        {
            yield return null;
            yield return null;

            GameFlowController flow = Object.FindAnyObjectByType<GameFlowController>();
            GameUiController ui = Object.FindAnyObjectByType<GameUiController>();
            Assert.NotNull(flow, "Boot did not create the game flow.");
            Assert.NotNull(ui, "Boot did not create the game UI.");

            Transform newFarm = ui.transform.Find("Canvas/MainMenu/MenuCard/NewFarm");
            Assert.NotNull(newFarm, "Main menu New Farm button is missing.");
            newFarm.GetComponent<Button>().onClick.Invoke();
            yield return null;

            Assert.NotNull(flow.World, "New Farm did not create FarmWorld.");
            Assert.IsTrue(flow.World.enabled, "Playable FarmWorld is disabled.");
            Assert.AreEqual(16, flow.World.Plots.Count, "Farm plot count does not match the complete game layout.");
            Assert.GreaterOrEqual(flow.World.Stations.Count, 10, "Production, animal, market, home, well, and expansion stations are incomplete.");

            Transform hud = ui.transform.Find("Canvas/GameplayHUD");
            Assert.NotNull(hud);
            Assert.IsTrue(hud.gameObject.activeSelf);
            string[] screens = { "SHOP", "BUILD", "ORDERS", "BAG", "MAP" };
            foreach (string screen in screens)
            {
                Transform button = hud.Find("BottomNavigation/" + screen);
                Assert.NotNull(button, screen + " navigation button is missing.");
                button.GetComponent<Button>().onClick.Invoke();
                yield return null;
                Assert.NotNull(ui.transform.Find("Canvas/ModalShade/Card"), screen + " screen did not open.");
            }

            Object.Destroy(Object.FindAnyObjectByType<PhumFarmApplication>()?.gameObject);
            Object.Destroy(GameServices.Instance?.gameObject);
            yield return null;
        }
    }
}
