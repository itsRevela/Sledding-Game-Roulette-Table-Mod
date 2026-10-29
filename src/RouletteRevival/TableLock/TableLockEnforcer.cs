using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace RouletteRevival.TableLock
{
    /// <summary>
    /// Host-side: keeps the spinner's synced spin time parked so games without the mod can never open the table (their
    /// betting menu is broken and would trap them in the table camera). A real spin briefly overwrites the value; the
    /// enforcer notes the real spin and re-parks it, within the same network tick in practice.
    /// </summary>
    public sealed class TableLockEnforcer
    {
        private const float SearchIntervalSeconds = 1f;

        private RouletteSpinner _spinner;
        private float _nextSearch;
        private bool _loggedLock;

        private static MelonLogger.Instance Log => Melon<RouletteRevivalMod>.Logger;

        public void Tick()
        {
            var spinner = FindSpinner();
            if (spinner == null)
            {
                _loggedLock = false;
                return;
            }

            var synced = spinner.sync_LastSpinNetworkTime;
            if (SpinLockRules.IsLocked(synced.Value))
            {
                return;
            }

            if (synced.Value > 0d)
            {
                // The game just spun for real; remember it before parking the value again.
                RealSpin.Started(spinner.spinTime);
            }

            synced.Value = SpinLockRules.LockedSpinTime;
            if (!_loggedLock)
            {
                _loggedLock = true;
                Log.Msg("Table locked for players without the mod (they bet with !bet and spin with !spin).");
            }
        }

        private RouletteSpinner FindSpinner()
        {
            if (_spinner != null)
            {
                return _spinner;
            }

            if (Time.unscaledTime < _nextSearch)
            {
                return null;
            }

            _nextSearch = Time.unscaledTime + SearchIntervalSeconds;
            _spinner = Object.FindObjectOfType<RouletteSpinner>();
            return _spinner;
        }
    }
}
