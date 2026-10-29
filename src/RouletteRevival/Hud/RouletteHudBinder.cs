using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace RouletteRevival.Hud
{
    /// <summary>
    /// Runs on every client (host included). When a roulette table exists, wires the betting HUD that ships inside the
    /// prefab into UiReferenceController (the scene's references were nulled when the feature was sunset), and unwires it
    /// when the table goes away so the game never touches destroyed UI.
    /// </summary>
    public sealed class RouletteHudBinder
    {
        private const float ScanIntervalSeconds = 0.25f;

        private UiReferenceController _boundUi;
        private RouletteTable _boundTable;
        private RouletteTable _rejectedTable;
        private bool _isBound;
        private float _nextScanTime;

        /// <summary>Panel of the currently bound table, used by <see cref="GamepadSelectionPatch"/>; null when unbound.</summary>
        internal static GameObject ActivePanel { get; private set; }

        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        public void Tick()
        {
            var ui = UiReferenceController.Instance;
            if (ui == null || (_boundUi != null && ui.Pointer != _boundUi.Pointer))
            {
                // Scene torn down: the old controller and its references are gone with it.
                ForgetBinding();
                if (ui == null)
                {
                    return;
                }
            }

            if (_isBound)
            {
                if (_boundTable != null)
                {
                    return;
                }

                Unbind(ui);
            }

            if (Time.unscaledTime < _nextScanTime)
            {
                return;
            }

            _nextScanTime = Time.unscaledTime + ScanIntervalSeconds;
            var table = Object.FindObjectOfType<RouletteTable>();
            if (table == null || (_rejectedTable != null && table.Pointer == _rejectedTable.Pointer))
            {
                return;
            }

            Bind(ui, table);
        }

        private void Bind(UiReferenceController ui, RouletteTable table)
        {
            if (!RouletteHudParts.TryFind(table, out var parts, out var missing))
            {
                Log.Error($"Roulette table found but its betting HUD is incomplete (missing: {string.Join(", ", missing)}). Leaving it unwired.");
                _rejectedTable = table;
                return;
            }

            PruneDestroyedButtons(ui);
            ColumnButtonFix.Apply(table);
            ui.rouletteBetAmountText = parts.BetAmountText;
            ui.rouletteCurrentTotalBetText = parts.TotalBetText;
            ui.rouletteMaxWinText = parts.MaxWinText;
            ui.rouletteCurrentOddsText = parts.OddsText;
            ui.rouletteIncreaseBetButton = parts.IncreaseButton;
            ui.rouletteDecreaseBetButton = parts.DecreaseButton;
            ui.rouletteClearBetsButton = parts.ClearButton;
            InteractionMenuWriter.SetRouletteMenu(ui, parts.Panel, parts.FirstSelectable);

            // The game's own setup: button listeners (fresh buttons per table, so they never double), selection tracking
            // and initial texts. Vanilla calls it once in Start, when the fields were still null, so it bailed out.
            ui.Roulette_Setup();

            _boundUi = ui;
            _boundTable = table;
            _isBound = true;
            ActivePanel = parts.Panel;
            Log.Msg("Roulette betting HUD wired up.");
        }

        private void Unbind(UiReferenceController ui)
        {
            PruneDestroyedButtons(ui);
            ui.rouletteBetAmountText = null;
            ui.rouletteCurrentTotalBetText = null;
            ui.rouletteMaxWinText = null;
            ui.rouletteCurrentOddsText = null;
            ui.rouletteIncreaseBetButton = null;
            ui.rouletteDecreaseBetButton = null;
            ui.rouletteClearBetsButton = null;
            InteractionMenuWriter.SetRouletteMenu(ui, null, null);
            ForgetBinding();
        }

        private void ForgetBinding()
        {
            _boundUi = null;
            _boundTable = null;
            _rejectedTable = null;
            _isBound = false;
            ActivePanel = null;
        }

        /// <summary>The game registers grid buttons but never removes them; destroyed ones would throw on the next clear.</summary>
        private static void PruneDestroyedButtons(UiReferenceController ui)
        {
            var buttons = ui.rouletteButtons;
            if (buttons == null)
            {
                return;
            }

            for (var i = buttons.Count - 1; i >= 0; i--)
            {
                if (buttons[i] == null)
                {
                    buttons.RemoveAt(i);
                }
            }
        }
    }
}
