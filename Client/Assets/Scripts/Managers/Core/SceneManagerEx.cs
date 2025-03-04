    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class SceneManagerEx : MonoBehaviour
    {
    
        private static SceneManagerEx _instance;
        public static SceneManagerEx Instance
        {
            get
            {
                if (_instance == null)
                {
                    // 씬에 SceneManagerEx 오브젝트가 없으면 생성
                    GameObject go = new GameObject("SceneManagerEx");
                    _instance = go.AddComponent<SceneManagerEx>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
        public bool IsSceneLoaded { get; set; } = false;
        public void LoadNewScene(Define.Scene type)
        {
            string sceneName = GetSceneName(type);
            SceneManager.LoadScene(sceneName);   
        }
        public void LoadScene(string name)
        {
            Managers.UI.SceneUI.Clear();
            Managers.Clear();
            // SceneManagerEx 인스턴스가 제대로 초기화되었는지 확인
            if (this != null)
            {
                StartCoroutine(LoadSceneCoroutine(name));
                //SceneManager.LoadScene(name);
            }
            else
            {
                Debug.LogError("SceneManagerEx instance is not initialized.");
            }
        }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (Managers.UI == null)
        {
            Debug.LogError("UIManager is not initialized.");
            yield break;
        }

        Managers.UI.ShowLoadingUI();

        if (CurrentScene != null)
        {
            CurrentScene.Clear();
        }
        else
        {
            Debug.LogError("CurrentScene is null.");
        }

        Debug.Log($"Attempting to load scene: {sceneName}");

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty.");
            yield break;
        }

        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not included in the build settings.");
            yield break;
        }

        // 같은 씬을 재로드하는 경우 언로드하지 않음
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
            if (unloadOperation != null)
            {
                while (!unloadOperation.isDone)
                {
                    yield return null;
                }
            }
        }

        // 이미 로드된 씬인 경우 바로 활성화
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.LogWarning($"Scene '{sceneName}' is already loaded. Re-activating.");
            Managers.UI.HideLoadingUI();
            yield break;
        }

        Debug.Log($"Attempting to load scene: {sceneName}");

        AsyncOperation asyncLoad = null;
        try
        {
            Debug.Log("Before LoadSceneAsync");
            asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            asyncLoad.allowSceneActivation = false;
            Debug.Log("After LoadSceneAsync");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception occurred while loading scene: {ex.Message}");
            yield break;
        }

        if (asyncLoad == null)
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
            yield break;
        }

        float startTime = Time.time;
        while (!asyncLoad.isDone)
        {
            float elapsedTime = Time.time - startTime;
            float progress = Mathf.Clamp01(elapsedTime / 5.0f);

            Managers.UI.SetLoadingText($"Loading... {(int)(progress * 100)}%");
            Managers.UI.SetLoadingImage("Textures/Images/StoryScene009");

            if (asyncLoad.progress >= 0.9f)
            {
                Managers.UI.SetLoadingText("Loading... 100%");
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        Managers.UI.HideLoadingUI();
    }
    private void EnsureSingleAudioListener()
        {
            AudioListener[] listeners = GameObject.FindObjectsOfType<AudioListener>();
            if (listeners.Length > 1)
            {
                for (int i = 1; i < listeners.Length; i++)
                {
                    Destroy(listeners[i]);
                }
            }
        }
        private bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == sceneName)
                {
                    return true;
                }
            }
            return false;
        }
        string GetSceneName(Define.Scene type)
        {
            string name = System.Enum.GetName(typeof(Define.Scene), type);
            return name;
        }

        public void Clear()
        {
            CurrentScene.Clear();
        }
    }
