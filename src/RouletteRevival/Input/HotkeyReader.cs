using System;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace RouletteRevival.Input
{
    /// <summary>Reads function-key presses through the Input System, falling back to legacy input if no keyboard device exists.</summary>
    public static class HotkeyReader
    {
        private static bool _legacyUnavailable;

        public static bool WasFunctionKeyPressed(int number)
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                var control = FunctionKeyControl(keyboard, number);
                return control != null && control.wasPressedThisFrame;
            }

            return WasPressedLegacy(number);
        }

        private static KeyControl FunctionKeyControl(Keyboard keyboard, int number) => number switch
        {
            1 => keyboard.f1Key,
            2 => keyboard.f2Key,
            3 => keyboard.f3Key,
            4 => keyboard.f4Key,
            5 => keyboard.f5Key,
            6 => keyboard.f6Key,
            7 => keyboard.f7Key,
            8 => keyboard.f8Key,
            9 => keyboard.f9Key,
            10 => keyboard.f10Key,
            11 => keyboard.f11Key,
            12 => keyboard.f12Key,
            _ => null,
        };

        private static bool WasPressedLegacy(int number)
        {
            if (_legacyUnavailable || number < HotkeyParser.MinFunctionKey || number > HotkeyParser.MaxFunctionKey)
            {
                return false;
            }

            try
            {
                return UnityEngine.Input.GetKeyDown(KeyCode.F1 + (number - 1));
            }
            catch (Exception ex)
            {
                // Legacy input is disabled in Input System-only builds; stop retrying every frame.
                _legacyUnavailable = true;
                Melon<RouletteRevivalMod>.Logger.Warning($"No keyboard available for the relocate hotkey: {ex.Message}");
                return false;
            }
        }
    }
}
