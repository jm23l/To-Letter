using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LetterRun2D.Editor
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string ScenePath="Assets/Scenes/LetterTown2D.unity";
        static ProjectSetup() { EditorApplication.delayCall+=FirstOpen; }
        private static void FirstOpen()
        {
            if(EditorApplication.isCompiling || EditorApplication.isUpdating) { EditorApplication.delayCall+=FirstOpen;return; }
            if(EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Generate();
        }
        [MenuItem("Letter Run 2D/게임 장면 열기")]
        public static void Open()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if(File.Exists(ScenePath)) EditorSceneManager.OpenScene(ScenePath); else Generate();
        }
        [MenuItem("Letter Run 2D/기본 장면 다시 만들기")]
        public static void Reset()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode) return;
            if(File.Exists(ScenePath) && !EditorUtility.DisplayDialog("레터런 2D","현재 2D 장면을 기본 배치로 되돌립니다. 수정한 배치가 사라집니다.","다시 만들기","취소"))return;
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Generate();
        }
        private static void Generate()
        {
            Directory.CreateDirectory("Assets/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=GameBootstrap.BuildScene();
            EditorSceneManager.SaveScene(scene,ScenePath);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
            EditorSettings.defaultBehaviorMode=EditorBehaviorMode.Mode2D;
            PlayerSettings.productName="LETTER RUN 2D";
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth=480;PlayerSettings.defaultScreenHeight=854;
            PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
            AssetDatabase.SaveAssets();Selection.activeGameObject=root;
            Debug.Log("레터런 2D 준비 완료. Game 창에서 Play를 누르세요.");
        }
    }
}
