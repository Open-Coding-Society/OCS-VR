using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace OCS.VR.Experience
{
    /// <summary>
    /// Puts the headset's tracking on a floor origin, so the viewer's real eye height
    /// stands on the hall floor at this object's position. Without it some runtimes start
    /// at device height and the viewer floats or sinks. Tries for a few seconds, since the
    /// XR subsystem can come up a frame or two after the scene. No allocation after Start.
    /// Does nothing in the editor without a headset, where the camera keeps its preview pose.
    /// </summary>
    public class XRFloorOrigin : MonoBehaviour
    {
        [Tooltip("Seconds to keep trying before giving up quietly.")]
        public float tryForSeconds = 5f;

        readonly List<XRInputSubsystem> _subsystems = new List<XRInputSubsystem>(2);
        float _giveUpAt;
        bool _done;

        void Start() => _giveUpAt = Time.time + tryForSeconds;

        void Update()
        {
            if (_done) return;
            if (Time.time > _giveUpAt) { _done = true; return; }

            SubsystemManager.GetSubsystems(_subsystems);
            for (int i = 0; i < _subsystems.Count; i++)
            {
                XRInputSubsystem s = _subsystems[i];
                if (s == null || !s.running) continue;
                if ((s.GetSupportedTrackingOriginModes() & TrackingOriginModeFlags.Floor) == 0) continue;
                if (s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor))
                {
                    _done = true;
                    Debug.Log("[XRFloorOrigin] Tracking origin set to the floor.");
                }
            }
        }
    }
}
