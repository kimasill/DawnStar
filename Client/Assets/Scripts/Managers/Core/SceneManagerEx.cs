using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : MonoBehaviour
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }

	public void LoadScene(Define.Scene type)
    {
        Managers.Clear();

        SceneManager.LoadScene(GetSceneName(type));
    }
    public void LoadScene(string name)
    {
        Managers.Clear();
        StartCoroutine(LoadSceneCoroutine(name));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // 로딩창 표시
        Managers.UI.ShowLoadingUI();

        // 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            // 로딩 진행 상황 업데이트
            Managers.UI.SetLoadingText($"Loading... {asyncLoad.progress * 100}%");
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
