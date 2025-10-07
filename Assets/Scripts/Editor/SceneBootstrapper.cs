#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace PlayingCard.Editor
{
    [InitializeOnLoad]
    public class SceneBootstrapper
    {
        const string Key_PreviousScene = "PreviousScene";
        const string Key_ShouldLoadBootstrapScene = "LoadBootstrapScene";

        const string Key_LoadBootstrapSceneOnPlay = "Playing Card/Load Bootstrap Scene On Play";
        const string Key_DoNotLoadBootstrapSceneOnPlay = "Playing Card/Don't Load Bootstrap Scene On Play";

        static bool IsRestartingToSwitchScene;

        static string BootstrapScene => EditorBuildSettings.scenes[0].path;

        static string PreviousScene
        {
            get => EditorPrefs.GetString(Key_PreviousScene);
            set => EditorPrefs.SetString(Key_PreviousScene, value);
        }

        static bool ShouldLoadBootstrapScene
        {
            get
            {
                if (!EditorPrefs.HasKey(Key_ShouldLoadBootstrapScene))
                {
                    EditorPrefs.SetBool(Key_ShouldLoadBootstrapScene, true);
                }

                return EditorPrefs.GetBool(Key_ShouldLoadBootstrapScene, true);
            }
            set => EditorPrefs.SetBool(Key_ShouldLoadBootstrapScene, value);
        }

        static SceneBootstrapper()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (!ShouldLoadBootstrapScene)
            {
                return;
            }

            if (IsRestartingToSwitchScene)
            {
                /// 플레이 모드에 진입하고 모든 작업이 완료된 후에만 stoppingAndStarting을 설정하도록 지정함.
                /// 이렇게 하면 여러 번의 시작 및 중지로 인해 "activeScene"이 손상되지 않으며, 처음에 편집하고 있던 씬으로 다시 돌아갈 수 있게 됨.
                if (playModeStateChange == PlayModeStateChange.EnteredPlayMode)
                {
                    IsRestartingToSwitchScene = false;
                }
                return;
            }

            if (playModeStateChange == PlayModeStateChange.ExitingEditMode)
            {
                // 이전 씬을 캐시하여, 가능하다면 플레이 세션이 끝난 후 이 씬으로 다시 돌아올 수 있도록 합니다.
                PreviousScene = EditorSceneManager.GetActiveScene().path;

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    // "Save" 또는 "Don't Save"을 선택했을 경우, 부트스트랩 씬을 엽니다.
                    if (!string.IsNullOrEmpty(BootstrapScene) &&
                        System.Array.Exists(EditorBuildSettings.scenes, scene => scene.path == BootstrapScene))
                    {
                        var activeScene = EditorSceneManager.GetActiveScene();

                        // 현재 씬이 빈 씬이거나,
                        // 활성 씬이 이미 BootstrapScene이 아닌 경우에만 Bootstrap 씬을 수동으로 삽입합니다.
                        IsRestartingToSwitchScene = activeScene.path == string.Empty || !BootstrapScene.Contains(activeScene.path);
                        if (IsRestartingToSwitchScene)
                        {
                            EditorApplication.isPlaying = false;

                            EditorSceneManager.OpenScene(BootstrapScene);

                            EditorApplication.isPlaying = true;
                        }
                    }
                }
                else
                {
                    // 사용자가 "Cancel"를 눌렀거나 창을 닫은 경우, 부트스트랩 씬을 열지 않고 에디터로 돌아갑니다.
                    EditorApplication.isPlaying = false;
                }
            }
            else if (playModeStateChange == PlayModeStateChange.EnteredEditMode)
            {
                if (!string.IsNullOrEmpty(PreviousScene))
                {
                    EditorSceneManager.OpenScene(PreviousScene);
                }
            }
        }

        [MenuItem(Key_LoadBootstrapSceneOnPlay, true)]
        static bool ShowLoadBootstrapSceneOnPlay()
        {
            return !ShouldLoadBootstrapScene;
        }

        [MenuItem(Key_LoadBootstrapSceneOnPlay)]
        static void EnableLoadBootstrapSceneOnPlay()
        {
            ShouldLoadBootstrapScene = true;
        }

        [MenuItem(Key_DoNotLoadBootstrapSceneOnPlay, true)]
        static bool ShowDoNotLoadBootstrapSceneOnPlay()
        {
            return ShouldLoadBootstrapScene;
        }

        [MenuItem(Key_DoNotLoadBootstrapSceneOnPlay)]
        static void DisableDoNotLoadBootstrapSceneOnPlay()
        {
            ShouldLoadBootstrapScene = false;
        }
    }
}
#endif