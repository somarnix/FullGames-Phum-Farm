using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhumFarm.Editor
{
    [InitializeOnLoad]
    public static class ProjectBootstrap
    {
        private const string ScenePath = "Assets/PhumFarm/Scenes/Boot.unity";

        static ProjectBootstrap() => EditorApplication.delayCall += EnsureBootScene;

        [MenuItem("Phum Farm/Rebuild Boot Scene")]
        public static void RebuildBootScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("PhumFarmApplication").AddComponent<PhumFarmApplication>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            SetBuildScene();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log("Phum Farm Boot scene rebuilt and added to Build Settings.");
        }

        private static void EnsureBootScene()
        {
            PlayerSettings.productName = "Phum Farm";
            PlayerSettings.companyName = "GSTECH";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.gstech.phumfarm");
            if (!File.Exists(ScenePath)) RebuildBootScene();
            else
            {
                SetBuildScene();
                if (!EditorApplication.isPlayingOrWillChangePlaymode && string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
                    EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }

        private static void SetBuildScene()
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }
    }
}
