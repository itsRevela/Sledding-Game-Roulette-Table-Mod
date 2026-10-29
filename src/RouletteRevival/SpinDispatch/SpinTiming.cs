using System;

namespace RouletteRevival.SpinDispatch
{
    /// <summary>
    /// The host sends the table's spin result a little later than the wheel's own timer. Games without the mod keep the
    /// grid buttons of every table they've seen, destroyed ones included, so their end-of-spin cleanup throws inside
    /// LeanTween's update. When that lands in the frame the ball's tween finishes, the tween (already rewound for removal)
    /// is never removed and the ball spins again. The offset moves the throw out of that frame; games with the mod undo it.
    /// </summary>
    public static class SpinTiming
    {
        /// <summary>Clear of the wheel's finish and of the result ball's 0.5 s bounce that starts with it.</summary>
        public const float ResultDelaySeconds = 0.25f;

        private const float Tolerance = 0.01f;

        public static float DelayedResultTime(float wheelSpinTime) => wheelSpinTime + ResultDelaySeconds;

        /// <summary>The result time this game should use: the wheel's own when the host delayed it, else what arrived.</summary>
        public static float LocalResultTime(float received, float wheelSpinTime) =>
            Math.Abs(received - DelayedResultTime(wheelSpinTime)) <= Tolerance ? wheelSpinTime : received;
    }
}
