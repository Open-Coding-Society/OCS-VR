using UnityEngine;

namespace OCS.VR.Experience
{
    /// <summary>
    /// The rules for walking the hall with a thumbstick: dead zone, a step in the
    /// direction the head faces, staying inside the hall, snap turns and rising or
    /// sinking. Plain C#, tested; XRLocomotion only reads the sticks and moves the rig.
    /// </summary>
    public static class LocomotionMath
    {
        /// <summary>
        /// Zero inside the dead zone, then rescaled so the stick still reaches full speed
        /// at full tilt. Keeps a resting thumb from drifting the viewer.
        /// </summary>
        public static Vector2 Deadzone(Vector2 stick, float deadzone)
        {
            float m = stick.magnitude;
            if (m <= deadzone || m <= 0f) return Vector2.zero;
            float scaled = Mathf.Min(1f, (m - deadzone) / Mathf.Max(1e-4f, 1f - deadzone));
            return stick * (scaled / m);
        }

        /// <summary>
        /// The step on the floor for a stick: up walks where the head faces, right
        /// sidesteps. Only the head's yaw counts, so looking down doesn't slow you.
        /// </summary>
        public static Vector3 Step(Vector2 stick, float headYawDegrees, float speed, float deltaTime)
        {
            float yaw = headYawDegrees * Mathf.Deg2Rad;
            float s = Mathf.Sin(yaw), c = Mathf.Cos(yaw);
            var forward = new Vector3(s, 0f, c);
            var right = new Vector3(c, 0f, -s);
            return (forward * stick.y + right * stick.x) * (speed * deltaTime);
        }

        /// <summary>
        /// Cuts a step short so the head stays inside the walkable area (x and z, metres).
        /// Each axis on its own, so walking into a wall at an angle slides along it.
        /// </summary>
        public static Vector3 KeepInside(Vector3 head, Vector3 step, Vector2 min, Vector2 max)
        {
            float x = Mathf.Clamp(head.x + step.x, Mathf.Min(min.x, head.x), Mathf.Max(max.x, head.x));
            float z = Mathf.Clamp(head.z + step.z, Mathf.Min(min.y, head.z), Mathf.Max(max.y, head.z));
            return new Vector3(x - head.x, 0f, z - head.z);
        }

        /// <summary>The rig's height after a frame of the stick pushed up or down, kept in range.</summary>
        public static float Climb(float height, float stickY, float speed, float deltaTime, float min, float max)
        {
            return Mathf.Clamp(height + stickY * speed * deltaTime, min, max);
        }

        /// <summary>
        /// One snap turn per flick: fires when the stick passes the threshold, then waits
        /// for it to come back near the middle before it can fire again.
        /// </summary>
        public struct SnapTurn
        {
            bool _waitingForCentre;

            /// <returns>Degrees to turn this frame: 0, +step or -step.</returns>
            public float Update(float x, float threshold, float release, float stepDegrees)
            {
                if (_waitingForCentre)
                {
                    if (Mathf.Abs(x) < release) _waitingForCentre = false;
                    return 0f;
                }
                if (x >= threshold) { _waitingForCentre = true; return stepDegrees; }
                if (x <= -threshold) { _waitingForCentre = true; return -stepDegrees; }
                return 0f;
            }
        }
    }
}
