using NUnit.Framework;
using UnityEngine;
using OCS.VR.Experience;

namespace OCS.VR.Tests
{
    public class LocomotionTests
    {
        const float Eps = 1e-4f;

        [Test]
        public void ARestingThumbDoesNotMove()
        {
            Assert.AreEqual(Vector2.zero, LocomotionMath.Deadzone(new Vector2(0.1f, 0.05f), 0.15f));
        }

        [Test]
        public void FullTiltStillReachesFullSpeed()
        {
            Vector2 v = LocomotionMath.Deadzone(new Vector2(0f, 1f), 0.15f);
            Assert.AreEqual(1f, v.y, Eps);
        }

        [Test]
        public void StickUpWalksWhereTheHeadFaces()
        {
            // Facing +z.
            Vector3 s = LocomotionMath.Step(new Vector2(0f, 1f), 0f, 2f, 0.5f);
            Assert.AreEqual(0f, s.x, Eps);
            Assert.AreEqual(1f, s.z, Eps);

            // Turned to face +x, the same push goes along +x.
            s = LocomotionMath.Step(new Vector2(0f, 1f), 90f, 2f, 0.5f);
            Assert.AreEqual(1f, s.x, Eps);
            Assert.AreEqual(0f, s.z, Eps);
        }

        [Test]
        public void StickRightSidesteps()
        {
            Vector3 s = LocomotionMath.Step(new Vector2(1f, 0f), 0f, 1f, 1f);
            Assert.AreEqual(1f, s.x, Eps);
            Assert.AreEqual(0f, s.z, Eps);
            Assert.AreEqual(0f, s.y, Eps);
        }

        [Test]
        public void WalkingIntoAWallSlidesAlongIt()
        {
            Vector3 step = LocomotionMath.KeepInside(new Vector3(3.9f, 1.6f, 0f), new Vector3(0.5f, 0f, 0.5f),
                                                     new Vector2(-4f, -6f), new Vector2(4f, 21f));
            Assert.AreEqual(0.1f, step.x, Eps);
            Assert.AreEqual(0.5f, step.z, Eps);
        }

        [Test]
        public void OutsideTheBoundsYouCanStillWalkBackIn()
        {
            // Stepped out by walking in the real room: walking back in is allowed,
            // walking further out is not.
            Vector3 head = new Vector3(5f, 1.6f, 0f);
            Vector2 min = new Vector2(-4f, -6f), max = new Vector2(4f, 21f);
            Assert.AreEqual(-0.5f, LocomotionMath.KeepInside(head, new Vector3(-0.5f, 0f, 0f), min, max).x, Eps);
            Assert.AreEqual(0f, LocomotionMath.KeepInside(head, new Vector3(0.5f, 0f, 0f), min, max).x, Eps);
        }

        [Test]
        public void ClimbingStopsAtTheFloorAndTheCeiling()
        {
            Assert.AreEqual(0f, LocomotionMath.Climb(0.1f, -1f, 1f, 1f, 0f, 1.5f), Eps);
            Assert.AreEqual(1.5f, LocomotionMath.Climb(1.4f, 1f, 1f, 1f, 0f, 1.5f), Eps);
            Assert.AreEqual(0.6f, LocomotionMath.Climb(0.5f, 0.5f, 0.2f, 1f, 0f, 1.5f), Eps);
        }

        [Test]
        public void OneFlickIsOneSnapTurn()
        {
            var snap = new LocomotionMath.SnapTurn();
            Assert.AreEqual(30f, snap.Update(0.9f, 0.7f, 0.3f, 30f));
            // Held over: no more turns until the stick comes back.
            Assert.AreEqual(0f, snap.Update(1f, 0.7f, 0.3f, 30f));
            Assert.AreEqual(0f, snap.Update(0.5f, 0.7f, 0.3f, 30f));
            Assert.AreEqual(0f, snap.Update(0.1f, 0.7f, 0.3f, 30f));
            Assert.AreEqual(-30f, snap.Update(-0.8f, 0.7f, 0.3f, 30f));
        }
    }
}
