using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using TMPro;
using UnityEngine;

public class ChestController : BaseController
{    
    private bool _isOpened = false;
    public string Name { get; private set; }
    public int TemplateId { get; private set; }
    private GameObject _headUpIcon;
    private TextMeshPro _headUpText;
    protected override void Init()
    {
        base.Init();        
    }
    public void SetChest(int id, int templateId)
    {
        Id = id;
        TemplateId = templateId;
        Managers.Data.AcquireDict.TryGetValue(templateId, out AcquireData data);
        if (data == null)
            return;
        Name = data.name;
    }
    public void ActivateNotification()
    {
        _headUpIcon = Managers.Resource.Instantiate("UI/HeadUpIcon", transform);
        if (_headUpIcon == null)
            return;        
        _headUpIcon.gameObject.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Chest");
        _headUpIcon.transform.localPosition = new Vector3(0, 1f, 0);
        _headUpText = _headUpIcon.GetComponentInChildren<TextMeshPro>();
        StartCoroutine(BlinkText(_headUpText));
    }

    private IEnumerator BlinkText(TextMeshPro text)
    {
        if (text == null)
            yield break;

        while (true)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.PingPong(Time.time, 1));
            yield return null;
        }
    }
    public void DeactivateNotification()
    {
        StopCoroutine(BlinkText(_headUpText));
        _headUpIcon?.SetActive(false);        
    }

    public void OpenChest()
    {
        if (_isOpened)
            return;

        _isOpened = true;
        C_OpenChest packet = new C_OpenChest() 
        { 
            ChestId = Id,
            TemplateId = TemplateId,
            PosX = PosInfo.PosX,
            PosY = PosInfo.PosY
        };
        Managers.Network.Send(packet);
        _animator.Play("OPEN_H");
        StartCoroutine(CoOpenChest());
        
        _animator.Play("CLOSE_H");
        StartCoroutine(CoCloseChest());
    }

    private IEnumerator CoOpenChest()
    {
        // 애니메이션 재생 시간 동안 대기
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length + 2000);
    }

    private IEnumerator CoCloseChest()
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}