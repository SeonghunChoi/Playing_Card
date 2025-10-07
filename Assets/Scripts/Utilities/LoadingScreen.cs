using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PlayingCard.Utilities
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        CanvasGroup canvasGroup;

        [SerializeField]
        Slider progressBar;

        [SerializeField]
        float fadeOutDuration = 0.5f;

        public AsyncOperation LocalLoadOperation;

        bool loadingScreenRunning;

        Coroutine fadeOutCoroutine;

        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            SetCanvasVisibility(false);
        }

        private void Update()
        {
            if (loadingScreenRunning)
            {
                progressBar.value = LocalLoadOperation.progress;
            }
        }

        public void StopLoadingScreen()
        {
            if (loadingScreenRunning)
            {
                if (fadeOutCoroutine != null)
                {
                    StopCoroutine(fadeOutCoroutine);
                }
                fadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
            }
        }

        public void StartLoadingScreen()
        {
            SetCanvasVisibility(true);
            loadingScreenRunning = true;
            UpdateLoadingScreen();
        }

        public void UpdateLoadingScreen()
        {
            if (loadingScreenRunning)
            {
                if (fadeOutCoroutine != null)
                {
                    StopCoroutine(fadeOutCoroutine);
                }
            }
        }

        void SetCanvasVisibility(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            canvasGroup.blocksRaycasts = visible;
        }

        IEnumerator FadeOutCoroutine()
        {
            loadingScreenRunning = false;

            float currentTime = 0;
            while (currentTime < fadeOutDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / fadeOutDuration);
                yield return null;
                currentTime += Time.deltaTime;
            }

            SetCanvasVisibility(false);
        }
    }
}
