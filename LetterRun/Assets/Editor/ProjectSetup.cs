using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TimeCourier.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/Scenes/VoxelCity.unity";
        static ProjectSetup() { EditorApplication.delayCall += FirstOpen; }
        private static void FirstOpen()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += FirstOpen; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
            Generate();
        }
        [MenuItem("Time Courier/Create or Reset Demo Scene")]
        public static void ResetDemo()
        {
            if (EditorApplication.isPlaying) return;
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog("Time Courier",
                "Replace the demo scene? Your edits to VoxelCity will be replaced.", "Replace", "Cancel")) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Generate();
        }
        private static void Generate()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameBootstrap.BuildScene();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.companyName = "Student Project";
            PlayerSettings.productName = "Time Courier Voxel 3D";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth = 480;
            PlayerSettings.defaultScreenHeight = 854;
            PlayerSettings.resizableWindow = true;
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = GameObject.Find("TimeCourier");
            Debug.Log("Time Courier Voxel 3D ready. Open Game view, choose a portrait aspect, and press Play.");
        }
        [MenuItem("Time Courier/Open Game Scene")]
        public static void OpenGameScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath);
            else Generate();
        }
    }
}
