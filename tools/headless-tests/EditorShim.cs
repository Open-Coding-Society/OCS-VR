// Minimal UnityEditor + extra UnityEngine surface, only so the editor scripts compile.
using System;
using System.Collections.Generic;

namespace UnityEditor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MenuItemAttribute : Attribute { public MenuItemAttribute(string path) { } public int priority; }

    public static class AssetDatabase
    {
        public static T LoadAssetAtPath<T>(string path) where T : UnityEngine.Object => null;
        public static void Refresh() { }
        public static void SaveAssets() { }
        public static string GetAssetPath(UnityEngine.Object o) => "";
        public static void CreateAsset(UnityEngine.Object o, string path) { }
        public static bool IsValidFolder(string path) => true;
        public static string CreateFolder(string parent, string name) => "";
    }

    public static class EditorUtility
    {
        public static void SetDirty(UnityEngine.Object o) { }
        public static bool DisplayDialog(string title, string message, string ok, string cancel) => true;
        public static int DisplayDialogComplex(string title, string message, string ok, string cancel, string alt) => 0;
    }
    [System.AttributeUsage(System.AttributeTargets.Class)] public class InitializeOnLoadAttribute : System.Attribute { }
    public enum PlayModeStateChange { EnteredEditMode, ExitingEditMode, EnteredPlayMode, ExitingPlayMode }

    public enum InteractionMode { AutomatedAction, UserAction }

    public static class PrefabUtility
    {
        public static UnityEngine.Object InstantiatePrefab(UnityEngine.Object o) => new UnityEngine.GameObject();
        public static UnityEngine.GameObject SaveAsPrefabAssetAndConnect(UnityEngine.GameObject go, string path, InteractionMode mode) => go;
    }

    public enum ModelImporterAnimationType { None, Legacy, Generic, Human }

    public class AssetImporter : UnityEngine.Object { }

    public class ModelImporter : AssetImporter
    {
        public bool generateSecondaryUV, importCameras, importLights, importAnimation, isReadable, preserveHierarchy;
        public ModelImporterAnimationType animationType;
    }

    public class AssetPostprocessor
    {
        public string assetPath = "";
        public AssetImporter assetImporter;
    }
}

namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup { EmptyScene, DefaultGameObjects }
    public enum NewSceneMode { Single, Additive }
    public enum OpenSceneMode { Single, Additive, AdditiveWithoutLoading }
    public struct Scene { }

    public static class EditorSceneManager
    {
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() => true;
        public static Scene NewScene(NewSceneSetup setup, NewSceneMode mode) => new Scene();
        public static Scene GetActiveScene() => new Scene();
        public static void MarkSceneDirty(Scene s) { }
        public static bool SaveScene(Scene s, string path) => true;
        public static Scene OpenScene(string path, OpenSceneMode mode) => new Scene();
    }
}

namespace UnityEditor
{
    public enum BuildTarget { StandaloneWindows64 = 19, Android = 13 }
    public enum BuildTargetGroup { Standalone = 1, Android = 7 }
    public enum ScriptingImplementation { Mono2x = 0, IL2CPP = 1 }
    [System.Flags] public enum AndroidArchitecture { None = 0, ARMv7 = 1, ARM64 = 2 }
    public enum AndroidSdkVersions { AndroidApiLevel29 = 29, AndroidApiLevel32 = 32 }
    public enum MobileTextureSubtarget { Generic = 0, ASTC = 5 }

    public static class PlayerSettings
    {
        public static string productName;
        public static UnityEngine.ColorSpace colorSpace;
        public static void SetApplicationIdentifier(Build.NamedBuildTarget t, string id) { }
        public static void SetScriptingBackend(Build.NamedBuildTarget t, ScriptingImplementation s) { }
        public static ScriptingImplementation GetScriptingBackend(Build.NamedBuildTarget t) => ScriptingImplementation.IL2CPP;
        public static void SetUseDefaultGraphicsAPIs(BuildTarget t, bool v) { }
        public static void SetGraphicsAPIs(BuildTarget t, UnityEngine.Rendering.GraphicsDeviceType[] apis) { }
        public static UnityEngine.Rendering.GraphicsDeviceType[] GetGraphicsAPIs(BuildTarget t) => new UnityEngine.Rendering.GraphicsDeviceType[0];
        public static class Android
        {
            public static AndroidArchitecture targetArchitectures;
            public static AndroidSdkVersions minSdkVersion;
        }
    }

    public static class EditorUserBuildSettings
    {
        public static BuildTarget activeBuildTarget;
        public static MobileTextureSubtarget androidBuildSubtarget;
        public static bool SwitchActiveBuildTarget(BuildTargetGroup g, BuildTarget t) => true;
    }

    public class EditorBuildSettingsScene
    {
        public bool enabled; public string path;
        public EditorBuildSettingsScene(string path, bool enabled) { this.path = path; this.enabled = enabled; }
    }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[0]; }

    public static class EditorApplication
    {
        public delegate void CallbackFunction();
        public static CallbackFunction update;
        public static CallbackFunction delayCall;
        public static bool isPlaying;
        public static event System.Action<PlayModeStateChange> playModeStateChanged;
    }
}

namespace UnityEditor.Build
{
    public struct NamedBuildTarget { public static readonly NamedBuildTarget Android = new NamedBuildTarget(); }
}

namespace UnityEditor.PackageManager
{
    public enum StatusCode { InProgress, Success, Failure }
    public class Error { public string message; }
    public class PackageInfo { public string packageId; }
    public static class Client { public static Requests.AddRequest Add(string id) => new Requests.AddRequest(); }
}

namespace UnityEditor.PackageManager.Requests
{
    public class AddRequest
    {
        public bool IsCompleted => true;
        public StatusCode Status => StatusCode.Success;
        public Error Error => null;
        public PackageInfo Result => new PackageInfo();
    }
}

namespace UnityEngine.Rendering
{
    public class RenderPipelineAsset : UnityEngine.ScriptableObject { }
    public static class GraphicsSettings { public static RenderPipelineAsset defaultRenderPipeline; }
}

namespace UnityEngine.Rendering.Universal
{
    public class ScriptableRendererData : UnityEngine.ScriptableObject { }
    public class UniversalRendererData : ScriptableRendererData { }
    public class UniversalRenderPipelineAsset : RenderPipelineAsset
    {
        public int msaaSampleCount; public bool supportsHDR; public float shadowDistance;
        public bool supportsCameraDepthTexture, supportsCameraOpaqueTexture, useSRPBatcher; public float renderScale;
        public static UniversalRenderPipelineAsset Create(ScriptableRendererData r = null) => new UniversalRenderPipelineAsset();
    }
}

namespace UnityEngine
{
    public static class QualitySettings
    {
        public static UnityEngine.Rendering.RenderPipelineAsset renderPipeline;
        public static string[] names = new string[0];
        public static int GetQualityLevel() => 0;
        public static void SetQualityLevel(int i, bool applyExpensiveChanges) { }
    }
}

namespace UnityEditor.Build
{
    public interface IOrderedCallback { int callbackOrder { get; } }
    public interface IPreprocessBuildWithReport : IOrderedCallback { void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report); }
    public class BuildFailedException : System.Exception { public BuildFailedException(string m) : base(m) { } }
}
namespace UnityEditor.Build.Reporting
{
    public class BuildReport { }
}
