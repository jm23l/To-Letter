using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LetterRun2D.Editor
{
    [InitializeOnLoad]
    public static class MenuPreviewSetup
    {
        static MenuPreviewSetup()
        {
            EditorApplication.delayCall+=ApplyOpenScenes;
            EditorSceneManager.sceneOpened+=OnSceneOpened;
        }
        private static void OnSceneOpened(Scene scene,OpenSceneMode mode)
        {EditorApplication.delayCall+=ApplyOpenScenes;}
        [MenuItem("Letter Run 2D/현재 장면에 새 시작 화면 적용")]
        public static void ApplyOpenScenes()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            if(EditorApplication.isCompiling || EditorApplication.isUpdating)
            {EditorApplication.delayCall+=ApplyOpenScenes;return;}
            for(int i=0;i<SceneManager.sceneCount;i++)
            {
                var scene=SceneManager.GetSceneAt(i);if(!scene.isLoaded)continue;
                foreach(var root in scene.GetRootGameObjects())
                foreach(var game in root.GetComponentsInChildren<DeliveryGame>(true))
                {
                    if(game.menuPanel==null || game.startButton==null)continue;
                    var styled=game.GetComponent<PostalUI>();
                    if(styled!=null && styled.HasLayout && game.letters.Length>=15 && game.targetDeliveries!=6 && game.targetDeliveries!=10)continue;
                    TownContentUpdate.Apply(game);
                    MobileControlsLayout.ApplyLayout(game);
                    PostalUI.Apply(game);
                    game.menuPanel.SetActive(true);
                    game.resultPanel.SetActive(false);
                    game.pausePanel.SetActive(false);
                    game.controlsPanel.SetActive(false);
                    EditorUtility.SetDirty(game);
                    EditorSceneManager.MarkSceneDirty(scene);
                    Debug.Log("새 시작 화면을 편집 장면에 적용했습니다. Ctrl+S로 장면을 저장하세요.",game);
                }
            }
            Canvas.ForceUpdateCanvases();SceneView.RepaintAll();EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
