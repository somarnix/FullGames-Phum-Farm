using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PhumFarm.Editor
{
    [InitializeOnLoad]
    public static class ProjectBootstrap
    {
        private const string SceneFolder = "Assets/PhumFarm/Scenes";
        private const string BootPath = SceneFolder + "/Boot.unity";
        private const string MainMenuPath = SceneFolder + "/MainMenu.unity";
        private const string FarmWorldPath = SceneFolder + "/FarmWorld.unity";
        private const string SettingsFolder = "Assets/PhumFarm/Settings";
        private const string PipelinePath = SettingsFolder + "/PhumFarmURP.asset";
        private const string RendererPath = SettingsFolder + "/PhumFarmRenderer.asset";

        static ProjectBootstrap() => EditorApplication.delayCall += EnsureProject;

        [MenuItem("Phum Farm/Rebuild Unity Project Foundation")]
        public static void RebuildProjectFoundation()
        {
            EnsureFolder(SceneFolder);
            CreateScene(BootPath, "Boot", true);
            CreateScene(MainMenuPath, "MainMenu", false);
            CreateScene(FarmWorldPath, "FarmWorld", false);
            SetBuildScenes();
            EnsureRenderPipeline();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(BootPath);
            Debug.Log("Phum Farm Unity scenes, URP, and Build Settings rebuilt.");
        }

        private static void EnsureProject()
        {
            PlayerSettings.productName = "Phum Farm";
            PlayerSettings.companyName = "GSTECH";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.gstech.phumfarm");
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.gstech.phumfarm");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.runInBackground = false;
            if (!File.Exists(BootPath) || !File.Exists(MainMenuPath) || !File.Exists(FarmWorldPath)) RebuildProjectFoundation();
            else
            {
                SetBuildScenes();
                EnsureRenderPipeline();
            }

            Scene active = EditorSceneManager.GetActiveScene();
            if (!Application.isBatchMode && !EditorApplication.isPlayingOrWillChangePlaymode && IsDefaultUntitledScene(active))
            {
                EditorSceneManager.OpenScene(BootPath, OpenSceneMode.Single);
            }
        }

        [MenuItem("Phum Farm/Open Boot Scene")]
        public static void OpenBootScene() => EditorSceneManager.OpenScene(BootPath, OpenSceneMode.Single);

        public static void OpenBootAndPlay()
        {
            OpenBootScene();
            EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
        }

        private static bool IsDefaultUntitledScene(Scene scene)
        {
            if (scene.name != "Untitled") return false;
            GameObject[] roots = scene.GetRootGameObjects();
            if (roots.Length > 2) return false;
            foreach (GameObject root in roots)
                if (root.name != "Main Camera" && root.name != "Directional Light") return false;
            return true;
        }

        private static void CreateScene(string path, string rootName, bool includeApplication)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject(rootName);
            if (includeApplication) new GameObject("PhumFarmApplication").AddComponent<PhumFarmApplication>();
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void SetBuildScenes()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootPath, true),
                new EditorBuildSettingsScene(MainMenuPath, true),
                new EditorBuildSettingsScene(FarmWorldPath, true)
            };
        }

        private static void EnsureRenderPipeline()
        {
            EnsureFolder(SettingsFolder);
            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                UniversalRendererData renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, RendererPath);
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "Phum Farm Mobile URP";
                pipeline.renderScale = 1f;
                pipeline.msaaSampleCount = 2;
                pipeline.supportsHDR = false;
                pipeline.shadowDistance = 45f;
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
                AssetDatabase.SaveAssets();
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[index]);
                current = next;
            }
        }
    }
}
