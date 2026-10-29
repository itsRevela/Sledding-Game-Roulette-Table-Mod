namespace RouletteRevival.TableLock
{
    /// <summary>
    /// Engine-free rules of the table lock. The game lets anyone use the table or wheel only while
    /// <c>lastSpinNetworkTime + spinTime</c> is in the past; the host parks that synced time far in the future so the
    /// unmodded game always believes the wheel is spinning. Modded games recognise the parked value and use the real
    /// spin state instead.
    /// </summary>
    public static class SpinLockRules
    {
        /// <summary>Network time the host parks the spinner's "last spin" at: centuries ahead of any real session.</summary>
        public const double LockedSpinTime = 1e12;

        /// <summary>Anything this far ahead can only be the lock, never a real spin timestamp.</summary>
        public const double LockThreshold = 1e11;

        public static bool IsLocked(double lastSpinNetworkTime) => lastSpinNetworkTime >= LockThreshold;

        /// <summary>Whether a real spin that ends at <paramref name="spinEndsAt"/> is still running at <paramref name="now"/>.</summary>
        public static bool IsRealSpinActive(float spinEndsAt, float now) => now < spinEndsAt;
    }
}
