using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace OCS.VR.Experience
{
    /// <summary>
    /// Starts the experience over from the empty bench: the menu button on the left
    /// controller, Start on a gamepad, R on the keyboard, or the Restart button in the
    /// corner of the Game view. Reloads the scene, so every
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
            if (_restart != null && _restart.WasPressedThisFrame()) RestartScene();
        }

        /// <summary>Reloads the scene. Also used by GazeRestart, the panel you look at.</summary>
        public static void RestartScene()
        {
            Time.timeScale = 1f;
            Scene scene = SceneManager.GetActiveScene();
            Debug.Log("[RestartButton] Starting over.");
            if (scene.buildIndex >= 0) SceneManager.LoadScene(scene.buildIndex);
            else SceneManager.LoadScene(scene.name);
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // A button in the corner of the Game view, for the desk. Not built for the headset,
        // where OnGUI draws nothing and would only cost time every frame.
        void OnGUI()
        {
            if (GUI.Button(new Rect(12f, 12f, 150f, 40f), "Restart  (R)")) RestartScene();
        }
#endif
    }
}
