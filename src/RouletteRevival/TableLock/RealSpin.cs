using UnityEngine;

namespace RouletteRevival.TableLock
{
    /// <summary>
    /// The wheel's real spinning state on this game, tracked from the spin results every game receives. Needed because the
    /// synced spin time is parked by the lock and no longer says when a real spin ends.
    /// </summary>
    public static class RealSpin
    {
        private static float _endsAt;

        public static bool IsSpinning => SpinLockRules.IsRealSpinActive(_endsAt, Time.unscaledTime);

        public static void Started(float spinTime)
        {
            var endsAt = Time.unscaledTime + spinTime;
            if (endsAt > _endsAt)
            {
                _endsAt = endsAt;
            }
        }

        public static void Reset() => _endsAt = 0f;
    }
}
