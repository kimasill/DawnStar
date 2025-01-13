using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Chat : UI_Base
{
    private TMP_InputField _chatInputField;
    private ScrollRect _chatScrollRect;
    private Transform _playerNameContent;
    private Transform _chatContent;
    private RectTransform _chatContentRect;
    private Queue<(GameObject, GameObject)> _chatMessages = new Queue<(GameObject, GameObject)>();
    private const int MaxChatMessages = 100;
    public Action CloseAction;
    public static bool IsChatting { get; private set; } = false;
    enum InputFields
    {
        ChatInputField,
    }
    enum Objects
    { 
        ChatPlayerNamePanel,
        ChatDisplayPanel,
    }

    enum Images
    {
        Chat_EnterButton,
        Chat_CloseButton,
    }

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));
        Bind<TMP_InputField>(typeof(InputFields));
        Bind<Image>(typeof(Images));
        _playerNameContent = GetObject((int)Objects.ChatPlayerNamePanel).transform;
        _chatContent = GetObject((int)Objects.ChatDisplayPanel).transform;
        _chatInputField = Get<TMP_InputField>((int)InputFields.ChatInputField);
        _chatScrollRect = GetComponentInChildren<ScrollRect>();
        _chatContentRect = _chatScrollRect.content.GetComponent<RectTransform>();

        GetImage((int)Images.Chat_EnterButton).gameObject.BindEvent(OnChatEnterButtonClicked);
        GetImage((int)Images.Chat_CloseButton).gameObject.BindEvent(OnChatCloseButtonClicked);
        _playerNameContent.gameObject.BindEvent(OnChatPlayerNamePanelClicked);
        _chatContent.gameObject.BindEvent(OnChatDisplayPanelClicked);        
        gameObject.BindEvent(OnBeginDrag, Define.UIEvent.BeginDrag);
        gameObject.BindEvent(OnDrag, Define.UIEvent.Drag);
        gameObject.BindEvent(OnEndDrag, Define.UIEvent.EndDrag);
        _chatInputField.onEndEdit.AddListener(OnChatInputEndEdit);
        _chatInputField.onSelect.AddListener(OnChatInputSelected);
        _chatInputField.onDeselect.AddListener(OnChatInputDeselected);
    }
    private void OnChatInputEndEdit(string message)
    {
        if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(message))
        {
            SendMessageToServer(message);
            _chatInputField.text = string.Empty;
            _chatInputField.ActivateInputField();
        }
    }
    private void SendMessageToServer(string message)
    {
        C_Chat chatPacket = new C_Chat();
        chatPacket.Message = message;
        Managers.Network.Send(chatPacket);
    }
    public void ApplyMessage(int id, string name, string message)
    {
        AddChatMessage(id, name, message);
    }
    private void AddChatMessage(int id, string playerName, string message)
    {
        if (_chatMessages.Count >= MaxChatMessages)
        {
            var (oldPlayerName, oldMessage) = _chatMessages.Dequeue();
            Destroy(oldPlayerName);
            Destroy(oldMessage);
        }

        GameObject playerNameObj = Managers.Resource.Instantiate("UI/UI_Text", _playerNameContent);
        UI_Text playerNameText = playerNameObj.GetComponent<UI_Text>();
        playerNameText.SetText(id, playerName);

        GameObject chatMessageObj = Managers.Resource.Instantiate("UI/UI_Text", _chatContent);
        UI_Text chatMessageText = chatMessageObj.GetComponent<UI_Text>();
        chatMessageText.SetText(id, message);

        _chatMessages.Enqueue((playerNameObj, chatMessageObj));
        UpdateContentSize();
        Canvas.ForceUpdateCanvases();
        _chatScrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases();
        
    }
    private void UpdateContentSize()
    {
        float totalHeight = 0f;
        foreach (var (playerNameObj, chatMessageObj) in _chatMessages)
        {
            totalHeight += playerNameObj.GetComponent<RectTransform>().rect.height;
        }
        _chatContentRect.sizeDelta = new Vector2(_chatContentRect.sizeDelta.x, totalHeight);
    }

    private void OnChatInputSelected(string message)
    {
        IsChatting = true;
    }

    private void OnChatInputDeselected(string message)
    {
        IsChatting = false;
    }

    private void OnChatPlayerNamePanelClicked(PointerEventData evt)
    {
        //정보요청
    }

    private void OnChatDisplayPanelClicked(PointerEventData evt)
    {
        //정보요청
    }

    private void OnChatEnterButtonClicked(PointerEventData evt)
    {
        if (!string.IsNullOrEmpty(_chatInputField.text))
        {
            SendMessageToServer(_chatInputField.text);
            _chatInputField.text = string.Empty;
            _chatInputField.ActivateInputField();
        }
    }

    private void OnChatCloseButtonClicked(PointerEventData evt)
    {
        CloseUI(evt);
    }

    public void OpenUI(PointerEventData eventData)
    {
        gameObject.SetActive(true);
    }
    public void CloseUI(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        CloseAction.Invoke();
    }
}


