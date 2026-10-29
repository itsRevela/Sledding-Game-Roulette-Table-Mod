using System.Collections.Generic;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteRevival.Hud
{
    /// <summary>The betting HUD components found under one spawned roulette table.</summary>
    public sealed class RouletteHudParts
    {
        private RouletteHudParts(GameObject panel, GameObject firstSelectable, TMP_Text betAmount, TMP_Text totalBet,
            TMP_Text maxWin, TMP_Text odds, Button increase, Button decrease, Button clear)
        {
            Panel = panel;
            FirstSelectable = firstSelectable;
            BetAmountText = betAmount;
            TotalBetText = totalBet;
            MaxWinText = maxWin;
            OddsText = odds;
            IncreaseButton = increase;
            DecreaseButton = decrease;
            ClearButton = clear;
        }

        /// <summary>"Roulette table menu interactions (off)": the menu panel the game toggles on Interact.</summary>
        public GameObject Panel { get; }

        /// <summary>First square of the betting grid, where gamepad selection starts.</summary>
        public GameObject FirstSelectable { get; }

        public TMP_Text BetAmountText { get; }
        public TMP_Text TotalBetText { get; }
        public TMP_Text MaxWinText { get; }
        public TMP_Text OddsText { get; }
        public Button IncreaseButton { get; }
        public Button DecreaseButton { get; }
        public Button ClearButton { get; }

        public static bool TryFind(RouletteTable table, out RouletteHudParts parts, out IReadOnlyList<string> missing)
        {
            var root = table.transform;
            missing = HudPaths.FindMissing(path => root.Find(path) != null);
            parts = null;
            if (missing.Count > 0)
            {
                return false;
            }

            var gridButton = root.Find(HudPaths.Canvas).GetComponentInChildren<RouletteButtonUI>(true);
            var found = new RouletteHudParts(
                root.Find(HudPaths.Panel).gameObject,
                gridButton != null ? gridButton.gameObject : null,
                root.Find(HudPaths.BetAmountText).GetComponent<TMP_Text>(),
                root.Find(HudPaths.TotalBetText).GetComponent<TMP_Text>(),
                root.Find(HudPaths.MaxWinText).GetComponent<TMP_Text>(),
                root.Find(HudPaths.OddsText).GetComponent<TMP_Text>(),
                root.Find(HudPaths.IncreaseBetButton).GetComponent<Button>(),
                root.Find(HudPaths.DecreaseBetButton).GetComponent<Button>(),
                root.Find(HudPaths.ClearBetsButton).GetComponent<Button>());

            if (!found.HasAllComponents)
            {
                missing = new[] { "a text or button component on the HUD objects" };
                return false;
            }

            parts = found;
            return true;
        }

        private bool HasAllComponents =>
            BetAmountText != null && TotalBetText != null && MaxWinText != null && OddsText != null
            && IncreaseButton != null && DecreaseButton != null && ClearButton != null;
    }
}
