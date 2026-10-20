using UnityEngine;

namespace OCS.VR.Experience
{
    /// <summary>
    /// The START AGAIN panel behind where the viewer starts: look at it for a couple of
    /// seconds and the experience starts over. Needs no controller, so it works for
    /// whoever has the headset on. A bar along the panel fills as you look.
    ///
    /// The timing rule is DwellTimer. No allocation in Update.
    /// </summary>
    public class GazeRestart : MonoBehaviour
    {
        [Tooltip("The camera the headset drives.")]
        public Transform head;

        [Tooltip("Scaled along x from 0 to 1 as the look fills. Its pivot is the bar's left end.")]
        public Transform fill;

        [Tooltip("Seconds of looking to restart.")]
        public float holdSeconds = 2f;

        [Tooltip("How far off centre the look can be, degrees.")]
        public float maxAngle = 12f;

        [Tooltip("Further away than this, metres, and looking doesn't count.")]
        public float maxDistance = 5f;

        DwellTimer _dwell;
        float _cosMax;

        void Awake()
        {
            _cosMax = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
            SetFill(0f);
        }

        void Update()
        {
            if (head == null) return;

            Vector3 toPanel = transform.position - head.position;
            float distance = toPanel.magnitude;
            bool looking = distance > 0.01f && distance < maxDistance
                           && Vector3.Dot(head.forward, toPanel / distance) >= _cosMax;

            // Real seconds, so fast forward doesn't shorten the look.
            bool fired = _dwell.Update(looking, Time.unscaledDeltaTime, holdSeconds);
            SetFill(_dwell.Progress(holdSeconds));
            if (fired) RestartButton.RestartScene();
        }

        void SetFill(float progress)
        {
            if (fill == null) return;
            Vector3 s = fill.localScale;
            s.x = Mathf.Max(0.0001f, progress);       // never exactly zero: a degenerate matrix
            fill.localScale = s;
        }
    }
}
