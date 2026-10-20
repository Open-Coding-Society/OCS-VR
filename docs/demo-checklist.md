# Demo checklist: from this repo to a working Quest 2 demo

Everything in the experience is written and checked without Unity (92 tests, the whole
arc simulated, the shaders compiled with glslang). None of it has been opened in Unity
or run on a headset since the data hall went in. This list is the shortest path to a
demo you can trust, in order. Each step says what you should see; stop at the first one
that doesn't match and fix it before going on.

## On the PC, in Unity (about an hour, mostly waiting)

1. **Pull.** `git pull` on `claude/nifty-cannon-b34hkf`. Unity recompiles.
   - See: no red errors in the Console. If Unity opens in Safe Mode, a script didn't
     compile: send the first error.

2. **Models.** Nothing to do: the 8 rig models are in the repo, in `Assets/Art/Models`
   (about 300 KB, so no Git LFS needed). Unity imports them when it opens the project.

3. **URP.** OCS > Quest > 0. Set Up URP for Quest.
   - See: "URP is active on every quality level" in the Console. This is what stops
     everything rendering magenta.

4. **Build the scene.** Delete `Assets/Prefabs/Environment.prefab` if it exists (an old
   room saved by hand would replace the data hall), then OCS > Build Experience Scene.
   - See: the Console says "23 parts from Assets/Art/Models" and "the default room".

5. **Watch it on the monitor.** Press Play. The full arc runs in about two and a half
   minutes. Check, in order:
   - the parts fly in on arcs with cyan trails and land with rings and sparks, each
     labelled for a few seconds, cards with their white dummy plugs facing you;
   - the tables sink, the cards light up one by one, the shockwave runs across the floor
     and the racks wake up behind it;
   - a track of light appears on the floor and the stations rise out of it one by one,
     in a horseshoe round the rig: the Student's desk front left, the AWS gate, the
     Broker's tower with Redis and RDS behind it, the NetBird portal on the right, and
     the GPU Rig sign behind the rig; the facts board appears on the right;
   - a ball of light runs along the floor track from station to station, slowly enough
     to follow, each sign flashing and its caption filling in; the student's screen,
     the timeline and the journey board appear;
   - GPUs 0 to 3 go orange, the beam lands on them, a light runs along them, tokens
     stream across to the student's screen and the answer types out; the floor ripples
     orange;
   - the answer runs back along the track and the Student station flashes.
   - Walk round with WASD (arrow keys turn and rise), hold F to fast forward, and press R or click Restart in
     the Game view's corner to start over: the same as the sticks, the right trigger and the menu button in the
     headset.
   - Nothing is pink or magenta, no text is mirrored, nothing floats in the wrong place.
   If something is off, a screenshot and the Console are all I need.

6. **Edit mode tests.** Window > General > Test Runner > EditMode > Run All. All pass.

7. **Quest setup.** OCS > Quest > 1. Install XR Packages (wait for both to finish), then
   OCS > Quest > 2. Apply Quest Build Settings (it switches to Android; that reimport
   takes a few minutes).

8. **Turn on OpenXR.** Edit > Project Settings > XR Plug-in Management:
   - Android tab: tick **OpenXR**.
   - OpenXR, Android tab: tick **Meta Quest Support**; Render Mode **Single Pass
     Instanced**; under Enabled Interaction Profiles, add **Oculus Touch Controller
     Profile** (without it the sticks and the restart button do nothing).
   - Project Validation: fix anything red (it has a Fix button for most).

9. **Check.** OCS > Quest > Check Setup. Every line says ok.

10. **Commit.** Commit `Assets/Settings`, `Assets/Scenes/Experience.unity` and the
    `ProjectSettings`/`Packages` changes, with their `.meta` files.

## On the headset

11. **Build and Run.** Headset plugged in by USB, developer mode on, "Allow USB debugging"
    accepted inside the headset. File > Build Profiles > Android > Build And Run.
    - See: the app starts in the headset by itself. The first build takes a while.

12. **Stand in the right place.** Stand where you want the viewer to be and hold the Meta
    button to recenter, facing the way the bench should be. The rig is about a metre in
    front of you; the floor should be under your feet.

13. **Watch the whole arc in the headset**, as in step 5. Also check:
    - it's comfortable: nothing pops in too close to your face, text is readable;
    - the controls: left stick walks, right stick turns (left and right) and rises
      (up and down), holding the right trigger fast forwards 4×, the menu button on the
      left controller starts it over. They are
      written on the floor at your feet;
    - **frame rate.** Turn on the Meta performance overlay (Meta Quest Developer Hub >
      Performance Overlay) and watch the fps through the power on wave and while tokens
      stream. It must stay at 72. If it doesn't, tell me where it dropped; the fixes in
      order are fewer racks, less dust, then fog off.

14. **Do it twice more**, from a cold start of the app, so the demo isn't luck.

## Things to know for the demo itself

- The request shown is a **hand-written placeholder trace of the target path** (the
  broker, Redis and RDS are still being built). The dashboard says the network hops are
  slowed down to be followed and the GPU part is real time; the boards show real
  durations. Swap in a recorded trace when the broker writes one.
- Four cards per replica is an assumption (`gpu_count` in `Assets/Data/sample-trace.json`).
- The experience plays once, about two and a half minutes, then holds, and the viewer
  can walk round the stations. For the next person, press the menu button on the left
  controller, or turn round and look at the START AGAIN panel for two seconds: either
  starts over from the empty bench.
