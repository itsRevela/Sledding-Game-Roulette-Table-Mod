using System;
using MelonLoader;
using RouletteRevival;
using RouletteRevival.Config;
using RouletteRevival.Hud;
using RouletteRevival.Input;
using RouletteRevival.Spawning;

[assembly: MelonInfo(typeof(RouletteRevivalMod), "Roulette Revival", ModVersion.Value, "itsRevela")]
[assembly: MelonGame("The Sledding Corporation", "Sledding Game")]

namespace RouletteRevival
{
    /// <summary>
    /// Restores the sunsetted roulette minigame. Host: spawns the shipped network prefab. Everyone: wires the prefab's
    /// betting HUD back into the game's UI controller so the table can actually be used.
    /// </summary>
    public sealed class RouletteRevivalMod : MelonMod
    {
        private RouletteSettings _settings;
        private RouletteTableSpawner _spawner;
        private TableRelocator _relocator;
        private RouletteHudBinder _hudBinder;
        private string _lastError;

        public override void OnInitializeMelon()
        {
            _settings = new RouletteSettings();
            _spawner = new RouletteTableSpawner();
            _relocator = new TableRelocator(_spawner, _settings);
            _hudBinder = new RouletteHudBinder();

            if (!_settings.IsRelocateKeyValid)
            {
                LoggerInstance.Warning($"RelocateKey '{_settings.RelocateKeyText}' is not F1-F12; using {RouletteSettings.DefaultRelocateKey}.");
            }

            LoggerInstance.Msg($"Loaded. Host a lobby to place the table; host presses F{_settings.RelocateFunctionKey} to move it.");
        }

        public override void OnUpdate()
        {
            if (_settings == null)
            {
                return;
            }

            // Always bind the HUD, even with spawning disabled: a modded host may still place a table in this lobby.
            Guarded("HUD binding", _hudBinder.Tick);

            if (!_settings.Enabled)
            {
                return;
            }

            Guarded("table spawning", () => _spawner.Tick(_settings.SpawnPose));
            Guarded("relocate hotkey", () =>
            {
                if (HotkeyReader.WasFunctionKeyPressed(_settings.RelocateFunctionKey))
                {
                    _relocator.RelocateToLocalPlayer();
                }
            });
        }

        /// <summary>Runs one per-frame step in isolation so a failure in one never blocks the others; logs each distinct failure once.</summary>
        private void Guarded(string step, Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var message = $"{step}: {ex}";
                if (message != _lastError)
                {
                    LoggerInstance.Error($"Roulette Revival {step} failed: {ex}");
                    _lastError = message;
                }
            }
        }
    }
}
