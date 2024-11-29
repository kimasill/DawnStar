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
    public Grade Grade { get; set; }
    public int ChestId { get; private set; }
    private GameObject _headUpIcon;
    private TextMeshPro _headUpText;
    IEnumerator _routine;
    protected override void Init()
    {
        Animator = GetComponent<Animator>();
        if (Animator != null)
            _animatorSpeed = Animator.speed;
        _sprite = GetComponent<SpriteRenderer>();

        CellPos = Managers.Map.CurrentGrid.WorldToCell(transform.position);
        UpdateSortingLayer();

        Animator.Play("OPEN",0,0);
        Animator.speed = 0;
    }
    public void SetChest(int chsetId, int templateId)
    {
        ChestId = chsetId;
        TemplateId = templateId;
        Managers.Data.AcquireDict.TryGetValue(templateId, out AcquireData data);
        if (data == null)
            return;
        Name = data.name;
        Grade = data.grade;
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
        if(gameObject.activeSelf)
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
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("ChestController or GameObject is already destroyed.");
            return;
        }
        StopCoroutine(BlinkText(_headUpText));
        if (_headUpIcon != null)
            _headUpIcon.SetActive(false);        
    }

    public void OpenChest()
    {
        if (_isOpened)
            return;
        _isOpened = true;
        Animator.speed = 1;

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
    private IEnumerator CoShakeChest()
    {
        Animator.Play("SHAKE");        
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
    }
    private IEnumerator CoOpenChest()
    {
        if (CheckAnimatorLayer("SHAKE") == true)
            yield return StartCoroutine(CoShakeChest());

        Animator.Play("OPEN");
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);

        Animator.speed = 0;
        yield return new WaitForSeconds(3f);
        if (CheckAnimatorLayer("CLOSE") == true)
            yield return StartCoroutine(CoCloseChest());
    }

    private IEnumerator CoCloseChest()
    {        
        Animator.speed = 1;
        Animator.Play("CLOSE");
        yield return new WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length - 0.05f);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    protected override void UpdateAnimation()
    {
    }
    private bool CheckAnimatorLayer(string layerName)
    {
        if (Animator == null)
            return false;

        for (int i = 0; i < Animator.layerCount; i++)
        {
            if (Animator.GetLayerName(i) == layerName)
                return true;
        }

        return false;
    }
}