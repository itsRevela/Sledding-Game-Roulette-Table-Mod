namespace RouletteRevival.ChatBetting
{
    /// <summary>How a roulette line is written into the game's chat (engine-free).</summary>
    public static class ChatLineFormat
    {
        public const string SenderName = "Roulette";

        /// <summary>Rich-text tag that renders everything after it at zero size, i.e. invisibly and taking no space.</summary>
        public const string HideRest = "<size=0>";

        public static string Line(string text) => $"{SenderName}: {text}";

        /// <summary>
        /// The name-field value for a "{Username} has joined the game" system message that should read as <paramref name="text"/>:
        /// the template's own wording after the name is hidden.
        /// </summary>
        public static string InSystemTemplate(string text) => Line(text) + HideRest;
    }
}
