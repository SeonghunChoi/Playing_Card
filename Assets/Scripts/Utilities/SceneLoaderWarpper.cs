using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayingCard.Utilities
{
    public class SceneLoaderWarpper : MonoBehaviour
    {
        [SerializeField]
        LoadingScreen loadingScreen;

        public static SceneLoaderWarpper Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            if (loadSceneMode == LoadSceneMode.Single)
            {
                loadingScreen.StartLoadingScreen();
                loadingScreen.LocalLoadOperation = loadOperation;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            loadingScreen.StopLoadingScreen();
        }
    }
}
