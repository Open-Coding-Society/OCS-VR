using UnityEngine;

namespace OCS.VR.Experience
{
    /// <summary>
    /// A button you press by looking at it: fills while you look, drains twice as fast when
    /// you look away, and fires once when full. Looking away and back is needed to fire
    /// again. Plain C#, tested; GazeRestart does the looking.
    /// </summary>
    public struct DwellTimer
    {
        float _held;
        bool _fired;

        /// <summary>How full it is, 0 to 1.</summary>
        public float Progress(float holdSeconds) => holdSeconds <= 0f ? 1f : Mathf.Clamp01(_held / holdSeconds);

        /// <returns>True on the one frame it fills.</returns>
        public bool Update(bool looking, float deltaTime, float holdSeconds)
        {
            if (!looking)
            {
                _held = Mathf.Max(0f, _held - deltaTime * 2f);
                if (_held <= 0f) _fired = false;
                return false;
            }
            if (_fired) return false;
            _held = Mathf.Min(holdSeconds, _held + deltaTime);
            if (_held < holdSeconds) return false;
            _fired = true;
            return true;
        }
    }
}
