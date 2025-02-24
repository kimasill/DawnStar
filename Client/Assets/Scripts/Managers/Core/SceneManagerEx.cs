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

        if (Managers.UI != null)
        {
            Managers.UI.ShowLoadingUI();
            yield return new WaitForSeconds(0.1f);
            // 추가 로직...
        }
        else
        {
            Debug.LogError("UIManager is not initialized.");
        }

        if (CurrentScene != null)
        {
            CurrentScene.Clear();
        }
        else
        {
            Debug.LogError("CurrentScene is null.");
        }

        Debug.Log($"Attempting to load scene: {sceneName}");

        // 씬 이름이 올바른지 확인
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty.");
            yield break;
        }

        // 씬이 빌드 설정에 포함되어 있는지 확인
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"Scene '{sceneName}' is not included in the build settings.");
            yield break;
        }
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.LogWarning($"Scene '{sceneName}' is already loaded.");
            yield break;
        }

        Debug.Log($"Attempting to load scene: {sceneName}");
        // 씬 로드 시작        
        AsyncOperation asyncLoad = null;
        try
        {
            Debug.Log("Before LoadSceneAsync");
            asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
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
        if (asyncLoad == null)
            {
                Debug.LogError($"Failed to load scene: {sceneName}");
                yield break;
            }

            float startTime = Time.time;
            while (!asyncLoad.isDone)
            {
                // 로딩 진행 상황 업데이트
                float elapsedTime = Time.time - startTime;
                float progress = Mathf.Clamp01(elapsedTime / 5.0f); // 5초 동안 로딩 진행

                Managers.UI.SetLoadingText($"Loading... {(int)(progress * 100)}%");
                Managers.UI.SetLoadingImage("Textures/Images/StoryScene009");

                if (asyncLoad.progress >= 1.0f || progress >= 1.0f)
                {
                    Managers.UI.SetLoadingText("Loading... 100%");
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // 씬 로드 완료 후 로딩창 숨기기
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
