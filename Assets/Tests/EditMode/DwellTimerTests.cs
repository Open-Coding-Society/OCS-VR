using NUnit.Framework;
using OCS.VR.Experience;

namespace OCS.VR.Tests
{
    public class DwellTimerTests
    {
        [Test]
        public void LookingLongEnoughFiresOnce()
        {
            var d = new DwellTimer();
            Assert.IsFalse(d.Update(true, 1f, 2f));
            Assert.AreEqual(0.5f, d.Progress(2f), 1e-4f);
            Assert.IsTrue(d.Update(true, 1f, 2f));
            Assert.IsFalse(d.Update(true, 1f, 2f));       // still looking: not again
        }

        [Test]
        public void AGlanceDoesNotFire()
        {
            var d = new DwellTimer();
            Assert.IsFalse(d.Update(true, 1.5f, 2f));
            Assert.IsFalse(d.Update(false, 0.5f, 2f));    // drains at twice the speed
            Assert.AreEqual(0.25f, d.Progress(2f), 1e-4f);
            Assert.IsFalse(d.Update(true, 1f, 2f));
        }

        [Test]
        public void LookingAwayAndBackFiresAgain()
        {
            var d = new DwellTimer();
            d.Update(true, 2f, 2f);
            d.Update(false, 1f, 2f);
            Assert.AreEqual(0f, d.Progress(2f), 1e-4f);
            Assert.IsTrue(d.Update(true, 2f, 2f));
        }
    }
}
