using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UI_Quest_Title : UI_Base
{
    [SerializeField]
    private TMP_Text _titleText = null;

    [SerializeField]
    private Image _background = null;

    [SerializeField]
    private UI_Quest UI_Quest { get; set; }

    public int TemplateId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public bool Completed { get; private set; }
    public int Progress { get; private set; }
    public string Description { get; private set; } = string.Empty;
    private Color _color;
    private bool _isClicked = false;
    public bool Clicked
    {
        get { return _isClicked; } 
        set
        {
            if (_isClicked == value)
                return;
            _isClicked = value;
            if (_isClicked == true)
            {
                _color = _background.color;
                _background.color = new Color(_background.color.r, _background.color.g, _background.color.b, _background.color.a / 2);
            }
            else
            {
                _background.color = _color;
            }
        }
    } 

    public override void Init()
    {
        UI_Quest = GetComponentInParent<UI_Quest>();
        _background = GetComponent<Image>();
        gameObject.BindEvent(OnClick);
    }

    public void SetQuest(Quest quest)
    {
        if (quest == null)
        {            
            TemplateId = 0;
            Title = "";
            Completed = false;
            Progress = 0;

            _titleText.gameObject.SetActive(false);
        }
        Clicked = false;
        TemplateId = quest.TemplateId;
        Title = quest.Title;
        Completed = quest.IsCompleted;
        Progress = quest.Progress;
        Description = quest.QuestDescription;

        _titleText.text = Title;
        _titleText.gameObject.SetActive(true);
    }

    public void OnClick(PointerEventData evt)
    {
        if (UI_Quest != null)
        {
            UI_Quest.OnQuestTitleClick(this);
        }
    }
}