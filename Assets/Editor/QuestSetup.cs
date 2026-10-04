using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

namespace OCS.VR.EditorTools
{
    /// <summary>
    /// Gets the project from "plays in the editor" to "runs on a Quest 2", in the order the
    /// menu numbers say. Packages go in through the Package Manager Client API, never by
    /// editing manifest.json. Settings follow docs/setup-runbook.md sections 6 and 7.
    ///
    /// The one step left by hand is ticking OpenXR (and the Meta Quest feature group) in
    /// Project Settings: those settings live in the XR packages' own assets, and Check Setup
    /// says exactly where to click.
    /// </summary>
    public static class QuestSetup
    {
        const string ScenePath = "Assets/Scenes/Experience.unity";
        const string AppId = "org.opencodingsociety.ocsvr";
        const string ProductName = "OCS Intelligence VR";

        static readonly string[] XrPackages = { "com.unity.xr.management", "com.unity.xr.openxr" };
        static Queue<string> _toInstall;
        static AddRequest _request;

        const string PipelineDir = "Assets/Settings";
        const string PipelinePath = PipelineDir + "/OCS Quest URP.asset";
        const string RendererPath = PipelineDir + "/OCS Quest URP Renderer.asset";

        /// <summary>
        /// Makes sure URP is actually the active pipeline. If the project's quality levels
        /// point at pipeline assets that never made it into the repo, nothing is active and
        /// every material, including the OCS shaders, renders magenta. Creates one URP asset
        /// tuned for a Quest 2 (4x MSAA, no HDR, no shadows, no depth or opaque texture) and
        /// assigns it everywhere. Leaves a working pipeline alone unless asked.
        /// </summary>
        [MenuItem("OCS/Quest/0. Set Up URP for Quest", priority = 99)]
        public static void SetUpUrp()
        {
            var current = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
            if (current != null && QualitySettings.renderPipeline != null && AssetDatabase.GetAssetPath(current) != PipelinePath)
            {
                bool replace = EditorUtility.DisplayDialog("URP is already set up",
                    "A pipeline asset is already active (" + current.name + "). Replace it with the Quest tuned one?",
                    "Replace", "Keep mine");
                if (!replace) return;
            }

            if (!AssetDatabase.IsValidFolder(PipelineDir)) AssetDatabase.CreateFolder("Assets", "Settings");

            var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>(PipelinePath);
            if (asset == null)
            {
                var renderer = ScriptableObject.CreateInstance<UnityEngine.Rendering.Universal.UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
                asset = UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(asset, PipelinePath);
            }

            // What a Quest 2 can afford at 72 fps. MSAA does the anti-aliasing; nothing in the
            // scene needs HDR, shadows, or the depth and opaque textures.
            asset.msaaSampleCount = 4;
            asset.supportsHDR = false;
            asset.shadowDistance = 0f;
            asset.supportsCameraDepthTexture = false;
            asset.supportsCameraOpaqueTexture = false;
            asset.renderScale = 1f;
            asset.useSRPBatcher = true;
            EditorUtility.SetDirty(asset);

            UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline = asset;
            int level = QualitySettings.GetQualityLevel();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = asset;
            }
            QualitySettings.SetQualityLevel(level, false);

            AssetDatabase.SaveAssets();
            Debug.Log("[QuestSetup] URP is active on every quality level: " + PipelinePath +
                      ". Commit Assets/Settings with its .meta files so it reaches every machine.");
        }

        [MenuItem("OCS/Quest/1. Install XR Packages", priority = 100)]
        public static void InstallXrPackages()
        {
            _toInstall = new Queue<string>(XrPackages);
            InstallNext();
        }

        static void InstallNext()
        {
            if (_toInstall == null || _toInstall.Count == 0)
            {
                Debug.Log("[QuestSetup] XR packages installed. Next: OCS > Quest > 2. Apply Quest Build Settings.");
                return;
            }
            string id = _toInstall.Dequeue();
            Debug.Log("[QuestSetup] Installing " + id + " ...");
            _request = Client.Add(id);
            EditorApplication.update += WaitForInstall;
        }

        static void WaitForInstall()
        {
            if (_request == null || !_request.IsCompleted) return;
            EditorApplication.update -= WaitForInstall;
            if (_request.Status == StatusCode.Success)
                Debug.Log("[QuestSetup] Installed " + _request.Result.packageId);
            else
                Debug.LogError("[QuestSetup] Install failed: " + (_request.Error != null ? _request.Error.message : "unknown error"));
            _request = null;
            InstallNext();
        }

        [MenuItem("OCS/Quest/2. Apply Quest Build Settings", priority = 101)]
        public static void ApplyBuildSettings()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool go = EditorUtility.DisplayDialog("Switch to Android",
                    "The Quest is an Android device. Switching the build target reimports assets, which can take a few minutes.",
                    "Switch now", "Cancel");
                if (!go) return;
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            var android = NamedBuildTarget.Android;
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(android, AppId);
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            PlayerSettings.colorSpace = ColorSpace.Linear;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            if (File.Exists(ScenePath))
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            else
                Debug.LogWarning("[QuestSetup] " + ScenePath + " doesn't exist yet. Run OCS > Build Experience Scene, then this again.");

            AssetDatabase.SaveAssets();
            Debug.Log("[QuestSetup] Quest build settings applied: Android, IL2CPP, ARM64, Vulkan, linear, ASTC, Experience scene. " +
                      "Next: OCS > Quest > Check Setup.");
        }

        [MenuItem("OCS/Quest/Check Setup", priority = 120)]
        public static void CheckSetup()
        {
            var report = new StringBuilder("[QuestSetup] Setup check\n");
            int missing = 0;

            var android = NamedBuildTarget.Android;
            bool urp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline is UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset
                       && QualitySettings.renderPipeline != null;
            Line(report, ref missing, urp, "URP is the active pipeline (otherwise everything is magenta)",
                 "OCS > Quest > 0. Set Up URP for Quest");
            Line(report, ref missing, EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android, "Build target is Android",
                 "OCS > Quest > 2. Apply Quest Build Settings");
            Line(report, ref missing, PlayerSettings.GetScriptingBackend(android) == ScriptingImplementation.IL2CPP, "IL2CPP scripting backend",
                 "OCS > Quest > 2. Apply Quest Build Settings");
            Line(report, ref missing, PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64, "ARM64 only",
                 "OCS > Quest > 2. Apply Quest Build Settings");
            var apis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
            Line(report, ref missing, apis.Length > 0 && apis[0] == GraphicsDeviceType.Vulkan, "Vulkan first",
                 "OCS > Quest > 2. Apply Quest Build Settings");
            bool sceneInBuild = false;
            foreach (var s in EditorBuildSettings.scenes) if (s.enabled && s.path == ScenePath) sceneInBuild = true;
            Line(report, ref missing, File.Exists(ScenePath), "Experience scene built", "OCS > Build Experience Scene");
            Line(report, ref missing, sceneInBuild, "Experience scene is in the build", "OCS > Quest > 2. Apply Quest Build Settings");
            Line(report, ref missing, Directory.Exists("Assets/Art/Models") && File.Exists("Assets/Art/Models/gpu_card.fbx"),
                 "Rig models in Assets/Art/Models (placeholders work too, but look rougher)",
                 "Unzip rig-models-v2.zip into the project, then rebuild the scene");
            Line(report, ref missing, Shader.Find("OCS/Glow") != null && Shader.Find("OCS/Grid Floor") != null && Shader.Find("OCS/Rack Lights") != null,
                 "OCS shaders compiled", "Check the Console for a shader error in Assets/Shaders");

            bool openXrPackage = Type.GetType("UnityEngine.XR.OpenXR.OpenXRLoader, Unity.XR.OpenXR") != null;
            Line(report, ref missing, openXrPackage, "OpenXR package installed", "OCS > Quest > 1. Install XR Packages");
            if (openXrPackage)
            {
                bool? loader = OpenXrEnabledForAndroid();
                Line(report, ref missing, loader == true, "OpenXR enabled for Android",
                     loader == null
                        ? "Couldn't read it from here: open Project Settings > XR Plug-in Management, Android tab, and tick OpenXR"
                        : "Project Settings > XR Plug-in Management > Android tab: tick OpenXR, then tick the Meta Quest feature group");
                report.Append("  check  Project Settings > XR Plug-in Management > OpenXR > Android tab: Meta Quest Support ticked,\n" +
                              "         Render Mode = Single Pass Instanced, and Project Validation shows no red errors\n");
            }

            report.Append(missing == 0 ? "All set: File > Build Profiles > Android > Build And Run with the headset plugged in."
                                       : missing + " thing(s) to do.");
            if (missing == 0) Debug.Log(report.ToString()); else Debug.LogWarning(report.ToString());
        }

        static void Line(StringBuilder report, ref int missing, bool ok, string what, string fix)
        {
            report.Append(ok ? "  ok    " : "  TODO  ").Append(what);
            if (!ok) { report.Append("\n          → ").Append(fix); missing++; }
            report.Append('\n');
        }

        /// <summary>
        /// Whether OpenXR is an active loader for Android, read by reflection so this file
        /// compiles before the XR packages are installed. Null when it can't tell.
        /// </summary>
        static bool? OpenXrEnabledForAndroid()
        {
            try
            {
                Type perTarget = Type.GetType("UnityEditor.XR.Management.XRGeneralSettingsPerBuildTarget, Unity.XR.Management.Editor");
                if (perTarget == null) return null;
                var forTarget = perTarget.GetMethod("XRGeneralSettingsForBuildTarget",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                object general = forTarget?.Invoke(null, new object[] { BuildTargetGroup.Android });
                if (general == null) return false;
                object manager = general.GetType().GetProperty("Manager")?.GetValue(general);
                if (manager == null) return false;
                var loaders = manager.GetType().GetProperty("activeLoaders")?.GetValue(manager) as System.Collections.IEnumerable;
                if (loaders == null) return null;
                foreach (object l in loaders)
                    if (l != null && l.GetType().Name.Contains("OpenXR")) return true;
                return false;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
