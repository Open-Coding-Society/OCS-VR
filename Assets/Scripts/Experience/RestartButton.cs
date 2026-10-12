using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace OCS.VR.Experience
{
    /// <summary>
    /// Starts the experience over from the empty bench: the menu button on the left
    /// controller, Start on a gamepad, or R on the keyboard. Reloads the scene, so every
    /// act, light and label is back where it began, for the next person in the headset.
    /// </summary>
    public class RestartButton : MonoBehaviour
    {
        InputAction _restart;

        void Awake()
        {
            _restart = new InputAction("Restart", InputActionType.Button);
            _restart.AddBinding("<XRController>{LeftHand}/{MenuButton}");
            _restart.AddBinding("<Gamepad>/start");
            _restart.AddBinding("<Keyboard>/r");
        }

        void OnEnable() => _restart?.Enable();
        void OnDisable() => _restart?.Disable();
        void OnDestroy() => _restart?.Dispose();

        void Update()
        {
            if (_restart == null || !_restart.WasPressedThisFrame()) return;
            Scene scene = SceneManager.GetActiveScene();
            Debug.Log("[RestartButton] Starting over.");
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }
    }
}
