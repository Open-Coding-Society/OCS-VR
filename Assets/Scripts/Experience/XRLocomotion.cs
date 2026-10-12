using UnityEngine;
using UnityEngine.InputSystem;

namespace OCS.VR.Experience
{
    /// <summary>
    /// Walk the hall with the left stick, snap turn with the right stick left and right,
    /// rise or sink with it up and down. Moves the XR rig, never the camera: the headset
    /// keeps tracking the head inside it. In the editor, WASD walks, the arrow keys turn
    /// and rise, and a gamepad works too.
    ///
    /// The rules live in LocomotionMath. No allocation after Awake.
    /// </summary>
    public class XRLocomotion : MonoBehaviour
    {
        [Tooltip("The camera the headset drives. Walking goes where it faces.")]
        public Transform head;

        [Tooltip("Metres per second at full tilt. Slow is comfortable in a headset.")]
        public float walkSpeed = 1.4f;

        [Tooltip("Degrees per snap turn.")]
        public float snapDegrees = 30f;

        [Tooltip("Metres per second up or down.")]
        public float climbSpeed = 0.8f;

        [Tooltip("How far the viewer can rise above the floor, metres. Zero is standing on it.")]
        public float maxHeight = 1.5f;

        [Tooltip("The walkable floor, x and z, metres. Keeps the viewer inside the hall.")]
        public Vector2 boundsMin = new Vector2(-4f, -6f);
        public Vector2 boundsMax = new Vector2(4f, 21f);

        public float deadzone = 0.15f;

        InputAction _move;
        InputAction _look;
        LocomotionMath.SnapTurn _snap;

        void Awake()
        {
            _move = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");
            _move.AddBinding("<XRController>{LeftHand}/{Primary2DAxis}");
            _move.AddBinding("<Gamepad>/leftStick");
            _move.AddCompositeBinding("2DVector")
                 .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                 .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");

            _look = new InputAction("Turn and Rise", InputActionType.Value, expectedControlType: "Vector2");
            _look.AddBinding("<XRController>{RightHand}/{Primary2DAxis}");
            _look.AddBinding("<Gamepad>/rightStick");
            _look.AddCompositeBinding("2DVector")
                 .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                 .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        }

        void OnEnable()
        {
            _move?.Enable();
            _look?.Enable();
        }

        void OnDisable()
        {
            _move?.Disable();
            _look?.Disable();
        }

        void OnDestroy()
        {
            _move?.Dispose();
            _look?.Dispose();
        }

        void Update()
        {
            if (head == null) return;
            float dt = Time.deltaTime;

            Vector2 move = LocomotionMath.Deadzone(_move.ReadValue<Vector2>(), deadzone);
            if (move.sqrMagnitude > 0f)
            {
                Vector3 step = LocomotionMath.Step(move, head.eulerAngles.y, walkSpeed, dt);
                transform.position += LocomotionMath.KeepInside(head.position, step, boundsMin, boundsMax);
            }

            // Whichever way the right stick is pushed more decides: turn or rise.
            Vector2 look = LocomotionMath.Deadzone(_look.ReadValue<Vector2>(), deadzone);
            bool turning = Mathf.Abs(look.x) >= Mathf.Abs(look.y);
            float turn = _snap.Update(turning ? look.x : 0f, 0.7f, 0.3f, snapDegrees);
            if (turn != 0f) transform.RotateAround(head.position, Vector3.up, turn);

            if (!turning)
            {
                Vector3 p = transform.position;
                p.y = LocomotionMath.Climb(p.y, look.y, climbSpeed, dt, 0f, maxHeight);
                transform.position = p;
            }
        }
    }
}
