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
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();

        SceneManager.LoadScene(GetSceneName(type));
    }
    public void LoadScene(string name)
    {
        Managers.Clear();

        // SceneManagerEx 인스턴스가 제대로 초기화되었는지 확인
        if (this != null)
        {
            StartCoroutine(LoadSceneCoroutine(name));
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
        // 로딩창 표시
        Managers.UI.ShowLoadingUI();

        // 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
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
