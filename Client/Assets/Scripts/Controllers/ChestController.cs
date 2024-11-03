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
    public int ChestId { get; private set; }
    private GameObject _headUpIcon;
    private TextMeshPro _headUpText;
    protected override void Init()
    {
        base.Init();
        _animator.Play("OPEN_H",0,0);
        _animator.speed = 0;
    }
    public void SetChest(int chsetId, int templateId)
    {
        ChestId = chsetId;
        TemplateId = templateId;
        Managers.Data.AcquireDict.TryGetValue(templateId, out AcquireData data);
        if (data == null)
            return;
        Name = data.name;
    }
    public void ActivateNotification()
    {
        Debug.Log("Activate Chest Notification");
        if(_headUpIcon == null)
            _headUpIcon = Managers.Resource.Instantiate("UI/HeadUpIcon", transform);
        
        _headUpIcon.SetActive(true);
        _headUpIcon.gameObject.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Chest");
        _headUpIcon.transform.position = new Vector3(transform.position.x, transform.position.y + 1f, 0);

        if(_headUpText == null)
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
        Debug.Log("Deactivate Chest Notification");
        StopCoroutine(BlinkText(_headUpText));
        _headUpIcon?.SetActive(false);        
    }

    public void OpenChest()
    {
        if (_isOpened)
            return;
        _isOpened = true;
        _animator.speed = 1;
        DeactivateNotification();
        Destroy(_headUpIcon);
        C_OpenChest packet = new C_OpenChest() 
        { 
            ChestId = ChestId,
            TemplateId = TemplateId,
            PosX = PosInfo.PosX,
            PosY = PosInfo.PosY
        };
        Managers.Network.Send(packet);
        
        StartCoroutine(CoOpenChest());
    }

    private IEnumerator CoOpenChest()
    {
        _animator.Play("OPEN_H");
        // 애니메이션 재생 시간 동안 대기
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);        
        _animator.Play("CLOSE_H", 0, 0);
        _animator.speed = 0;
        yield return new WaitForSeconds(3f);
        
        yield return StartCoroutine(CoCloseChest());
    }

    private IEnumerator CoCloseChest()
    {
        _animator.speed = 1;
        _animator.Play("CLOSE_H");
        yield return new WaitForSeconds(0.01f);
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length - 0.05f);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}