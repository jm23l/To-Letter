using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TimeCourier.Editor
{
    [InitializeOnLoad]
    public static class KoreanSceneSetup
    {
        static KoreanSceneSetup()
        {
            EditorApplication.delayCall += ApplyWhenReady;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        { EditorApplication.delayCall += ApplyWhenReady; }

        private static void ApplyWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            { EditorApplication.delayCall += ApplyWhenReady; return; }
            Apply(false);
        }

        [MenuItem("Time Courier/한글 UI를 장면에 적용")]
        public static void ApplyFromMenu() { Apply(true); }

        private static void Apply(bool force)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var font = Resources.Load<Font>("Fonts/CourierKorean");
            if (font == null)
            {
                Debug.LogWarning("한글 폰트를 먼저 넣으세요: Assets/Resources/Fonts/CourierKorean.ttf");
                return;
            }
            foreach (var game in Resources.FindObjectsOfTypeAll<DeliveryGame>())
            {
                var scene = game.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded || EditorUtility.IsPersistent(game)) continue;
                var texts = game.GetComponentsInChildren<Text>(true);
                bool needsUpdate = force;
                foreach (var text in texts)
                    if (text.font != font)
                        needsUpdate = true;
                Undo.RegisterFullObjectHierarchyUndo(game.gameObject, "게임 시작 화면 적용");
                bool menuChanged = GameplayExpansion.ConfigureStartMenu(game);
                if (!needsUpdate && !menuChanged) continue;
                if (needsUpdate) GameplayExpansion.LocalizeUI(game);
                foreach (var text in texts)
                {
                    EditorUtility.SetDirty(text);
                    if (PrefabUtility.IsPartOfPrefabInstance(text))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(text);
                }
                EditorSceneManager.MarkSceneDirty(scene);
                Debug.Log("한글 UI와 게임 시작 버튼을 장면에 적용했습니다. Ctrl+S로 장면을 저장하세요.", game);
            }
        }
    }
}
