using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InteractionController : BaseController
{
    protected bool _isInteracted = false;
    public bool CanInteract { get; set; } = true;
    private GameObject _headUpIcon;
    private TextMeshPro _headUpText;    
    public int TemplateId { get; private set; }
    public bool Multi { get; set; }
    public InteractionType Type { get; set; }
    public List<string> Scripts { get; set; } = new List<string>();
    public List<Vector2Int> CellPoses { get; set; } = new List<Vector2Int>();
    public InteractionData InteractionData { get; set; }
    protected CameraController _cameraController;
    protected override void Init()
    {
        Animator = GetComponent<Animator>();
        if (Animator != null)
            _animatorSpeed = Animator.speed;
        _sprite = GetComponent<SpriteRenderer>();
        _cameraController = Managers.Scene.CurrentScene.MainCamera.GetComponent<CameraController>();
        if(Managers.Map!= null)
            CellPos = Managers.Map.CurrentGrid.WorldToCell(transform.position);
        UpdateSortingLayer();
    }

    public void SetInteraction(int interactionId)
    {
        TemplateId = interactionId;
        Managers.Data.InteractionDict.TryGetValue(interactionId, out InteractionData data);
        if (data == null)
            return;
        InteractionData = data;
        Multi = data.multi;
        Type = data.interactionType;
        Scripts = data.script;
        switch (Type)
        {
            case InteractionType.Door:
                HandleDoorInteraction(data);
                break;
            case InteractionType.Trigger:
                HandleTriggerInteraction(data);
                break;
            case InteractionType.ItemTable:
                HandleItemTableInteraction(data);
                break;
            case InteractionType.Quest:
                HandleQuestInteraction(data);
                break;
            default:
                Debug.LogWarning($"Unknown InteractionType: {Type}");
                break;
        }
        
    }
    protected virtual void HandleDoorInteraction(InteractionData data)
    {
        // Door 타입에 대한 추가 설정 로직
        // 예: 특정 애니메이션 설정, 사운드 설정 등
        Debug.Log("Handling Door Interaction");
    }

    protected virtual void HandleTriggerInteraction(InteractionData data)
    {
        // Trigger 타입에 대한 추가 설정 로직
        // 예: 특정 이벤트 트리거 설정 등
        Debug.Log("Handling Trigger Interaction");
    }

    protected virtual void HandleItemTableInteraction(InteractionData data)
    {
        // ItemTable 타입에 대한 추가 설정 로직
        // 예: 아이템 리스트 설정 등
        Debug.Log("Handling ItemTable Interaction");
    }
    protected virtual void HandleQuestInteraction(InteractionData data)
    {
    }
    protected override void UpdateAnimation()
    {
    }

    public void ActivateNotification()
    {
        Debug.Log("Activate Interaction Notification");
        if (_headUpIcon == null)
            _headUpIcon = Managers.Resource.Instantiate("UI/HeadUpIcon", transform);

        _headUpIcon.SetActive(true);
        if(Type == InteractionType.Door)
        {
            _headUpIcon.gameObject.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Door");
            _headUpIcon.transform.position = new Vector3(transform.position.x, transform.position.y + 0.8f, 0);
        }
        else if(Type == InteractionType.Trigger)
        {
             _headUpIcon.gameObject.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Trigger");
            _headUpIcon.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, 0);
        }
        else if(Type == InteractionType.ItemTable)
        {
            _headUpIcon.gameObject.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Take");
            _headUpIcon.transform.position = new Vector3(transform.position.x, transform.position.y + 0.6f, 0);
        }
        else if(Type == InteractionType.Quest)
        {
            _headUpIcon.gameObject.GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>("Textures/Images/QuestIcons/Icon_Quest");
            _headUpIcon.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, 0);
        }


        if (_headUpText == null)
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
        Debug.Log("Deactivate Interaction Notification");
        StopCoroutine(BlinkText(_headUpText));
        _headUpIcon?.SetActive(false);
    }
    public virtual void Interact(bool success,  bool action, List<int> ids = null)
    {
    }
    public virtual void Interact()
    {
        _isInteracted = false;
        CanInteract = false;
    }
    public void StartInteraction()
    {
        if (_isInteracted)
            return;
        _isInteracted = true;

        DeactivateNotification();

        if (Multi)
        {
            C_Interaction packet = new C_Interaction
            {
                ObjectId = TemplateId,
                InteractionType = Type
            };
            Managers.Network.Send(packet);
        }
        else
        {
            Interact();
        }
    }
    protected virtual void InteractAction()
    {
        UI_GameScene gameUI = Managers.UI.SceneUI as UI_GameScene;
        UI_GameWindow gameWindow = gameUI.GameWindow;
        if(Scripts != null)
        {
            gameWindow.ShowScript(Scripts);
        }
    }
    private IEnumerator CoInteract()
    {
        // 상호작용 애니메이션 재생
        // yield return new WaitForSeconds(애니메이션 재생 시간);

        // 상호작용 후 처리 코드
        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(CoEndInteraction());
    }
    private IEnumerator CoEndInteraction()
    {
        // 상호작용 종료 애니메이션 재생
        yield return new WaitForSeconds(3f);
        // yield return new WaitForSeconds(애니메이션 재생 시간);

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
    public virtual void DeactivateInteraction()
    {
        CanInteract = false;
    }
    protected virtual IEnumerator InteractionCameraMove(Transform target)
    {
        yield return StartCoroutine(_cameraController.MoveToPosition(target));
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(_cameraController.ResetCameraAndTarget(3.0f));
    }
}