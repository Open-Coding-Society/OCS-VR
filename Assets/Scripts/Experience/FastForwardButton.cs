using UnityEngine;
using UnityEngine.InputSystem;

namespace OCS.VR.Experience
{
    /// <summary>
    /// Hold to fast forward: the right trigger in the headset, the right trigger on a
    /// gamepad, or F on the keyboard. Everything that runs on game time speeds up together
    /// (the acts, the assembly, the request, particles); let go and it is back to normal.
    /// Walking uses unscaled time, so it stays at walking pace.
    /// </summary>
    public class FastForwardButton : MonoBehaviour
    {
        [Tooltip("How many times faster while held.")]
        public float speed = 4f;

        [Tooltip("Optional. Shown while fast forwarding.")]
        public GameObject indicator;

        InputAction _hold;
        bool _fast;

        void Awake()
        {
            _hold = new InputAction("Fast Forward", InputActionType.Button);
            _hold.AddBinding("<XRController>{RightHand}/{TriggerButton}");
            _hold.AddBinding("<Gamepad>/rightTrigger");
            _hold.AddBinding("<Keyboard>/f");
            if (indicator != null) indicator.SetActive(false);
        }

        void OnEnable() => _hold?.Enable();

        void OnDisable()
        {
            _hold?.Disable();
            SetFast(false);         // never leave the next scene or the editor sped up
        }

        void OnDestroy() => _hold?.Dispose();

        void Update()
        {
            if (_hold == null) return;
            bool held = _hold.IsPressed();
            if (held != _fast) SetFast(held);
        }

        void SetFast(bool on)
        {
            _fast = on;
            Time.timeScale = on ? Mathf.Max(1f, speed) : 1f;
            if (indicator != null && indicator.activeSelf != on) indicator.SetActive(on);
        }
    }
}
