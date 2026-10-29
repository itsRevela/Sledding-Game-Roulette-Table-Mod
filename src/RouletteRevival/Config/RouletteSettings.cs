using MelonLoader;
using RouletteRevival.Input;
using RouletteRevival.Placement;

namespace RouletteRevival.Config
{
    /// <summary>MelonPreferences-backed settings (UserData/MelonPreferences.cfg, category [RouletteRevival]).</summary>
    public sealed class RouletteSettings
    {
        public const string CategoryId = "RouletteRevival";
        public const string DefaultRelocateKey = "F8";

        /// <summary>
        /// Ceiling for the chat stake limit: losses are charged 5,000 at a time, so this bounds a settlement to 2,000 charges
        /// per player.
        /// </summary>
        public const long MaxChatBetPerSpin = 10_000_000;

        private readonly MelonPreferences_Category _category;
        private readonly MelonPreferences_Entry<bool> _enabled;
        private readonly MelonPreferences_Entry<bool> _useCustomPosition;
        private readonly MelonPreferences_Entry<float> _x;
        private readonly MelonPreferences_Entry<float> _y;
        private readonly MelonPreferences_Entry<float> _z;
        private readonly MelonPreferences_Entry<float> _yaw;
        private readonly MelonPreferences_Entry<string> _relocateKey;
        private readonly MelonPreferences_Entry<bool> _chatBetting;
        private readonly MelonPreferences_Entry<long> _chatBetMaxPerSpin;
        private readonly MelonPreferences_Entry<bool> _blockTableWithoutMod;

        public RouletteSettings()
        {
            _category = MelonPreferences.CreateCategory(CategoryId, "Roulette Revival");
            _enabled = _category.CreateEntry("Enabled", true, "Enabled",
                "Spawn the roulette table when you host a lobby.");
            _useCustomPosition = _category.CreateEntry("UseCustomPosition", false, "Use custom position",
                "Set automatically when the host relocates the table with the hotkey.");
            var fallback = PlacementMath.DefaultPose;
            _x = _category.CreateEntry("PositionX", fallback.X, "Position X");
            _y = _category.CreateEntry("PositionY", fallback.Y, "Position Y");
            _z = _category.CreateEntry("PositionZ", fallback.Z, "Position Z");
            _yaw = _category.CreateEntry("Yaw", fallback.YawDegrees, "Yaw (degrees)");
            _relocateKey = _category.CreateEntry("RelocateKey", DefaultRelocateKey, "Relocate hotkey",
                "Host only: moves the table in front of you and remembers the spot. F1-F12.");
            _chatBetting = _category.CreateEntry("ChatBetting", true, "Chat betting",
                "Host only: players without the mod can bet by chat ('!bet red 5000', '!bets', '!clear', '!spin', '!help').");
            _chatBetMaxPerSpin = _category.CreateEntry("ChatBetMaxPerSpin", 500_000L, "Chat bet limit per spin",
                "Most one player can stake on a single spin through chat (0 to 10,000,000).");
            _blockTableWithoutMod = _category.CreateEntry("BlockTableWithoutMod", true, "Block table for players without the mod",
                "Host only: players without the mod can't open the table (their game's betting menu is broken and traps them in the "
                + "table camera). They bet with !bet and spin with !spin instead.");
        }

        public bool Enabled => _enabled.Value;

        public bool ChatBetting => _chatBetting.Value;

        public bool BlockTableWithoutMod => _blockTableWithoutMod.Value;

        /// <summary>Configured per-spin chat stake limit, clamped to 0..<see cref="MaxChatBetPerSpin"/>.</summary>
        public long ChatBetMaxPerSpin => System.Math.Clamp(_chatBetMaxPerSpin.Value, 0, MaxChatBetPerSpin);

        /// <summary>The configured pose, or the built-in default when none was saved or the saved one is invalid.</summary>
        public TablePose SpawnPose =>
            PlacementMath.ResolveConfigured(_useCustomPosition.Value, new TablePose(_x.Value, _y.Value, _z.Value, _yaw.Value));

        /// <summary>Function-key number for the relocate hotkey; falls back to F8 when the config value is not F1-F12.</summary>
        public int RelocateFunctionKey =>
            HotkeyParser.TryParseFunctionKey(_relocateKey.Value, out var key)
                ? key
                : int.Parse(DefaultRelocateKey.Substring(1));

        public bool IsRelocateKeyValid => HotkeyParser.TryParseFunctionKey(_relocateKey.Value, out _);

        public string RelocateKeyText => _relocateKey.Value;

        public void SaveCustomPose(TablePose pose)
        {
            if (!pose.IsValid)
            {
                Melon<RouletteRevivalMod>.Logger.Warning($"Refusing to save invalid table position {pose}.");
                return;
            }

            _x.Value = pose.X;
            _y.Value = pose.Y;
            _z.Value = pose.Z;
            _yaw.Value = pose.YawDegrees;
            _useCustomPosition.Value = true;
            _category.SaveToFile(false);
        }
    }
}
