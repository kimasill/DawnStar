using UnityEngine;
using UnityEngine.UI;

public class UI_Enhance : UI_Base
{
    public GameObject enhancePanel;
    public Button enhanceButton;        
    private Item selectedItem;

    enum Texts
    {
        EnhanceResultText
    }

    public override void Init()
    {       
        
    }
    public void OpenEnhanceUI(Item item)
    {
        selectedItem = item;
        enhancePanel.SetActive(true);
    }

    private void OnEnhanceButtonClicked()
    {
        if (selectedItem == null)
        {
            GetTextMeshPro((int)Texts.EnhanceResultText).text = "No item selected.";
            return;
        }

        bool success = EnhanceItem(selectedItem);
        GetTextMeshPro((int)Texts.EnhanceResultText).text = success ? "Enhancement successful!" : "Enhancement failed.";
    }

    private bool EnhanceItem(Item item)
    {
        // 강화 로직 구현 (성공 확률 등)
        float successRate = 0.5f; // 50% 성공 확률
        bool isSuccess = Random.value < successRate;

        if (isSuccess)
        {
        }

        return isSuccess;
    }

}