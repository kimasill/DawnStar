using Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot_Icon : UI_Base
{
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _cooldownImage;
    public TMP_Text KeyText;

    enum Texts
    {
        SkillSlot_KeyText
    }

    enum Images
    {
        SkillSlot_CoolDownImage
    }
    public SkillData SkillData { get; private set; }

    public bool IsInit = false;
    public override void Init()
    {
        if (IsInit == true) 
            return;
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        KeyText = GetTextMeshPro((int)Texts.SkillSlot_KeyText);
        _cooldownImage = GetImage((int)Images.SkillSlot_CoolDownImage);
        _cooldownImage.gameObject.SetActive(false);
        IsInit = true;
    }

    public void SetSkill(SkillData skillData)
    {
        Sprite skillIcon = Managers.Resource.Load<Sprite>($"{skillData.icon}");
        SkillData = skillData;
        _skillIcon.sprite = skillIcon;
        gameObject.SetActive(true);
    }

    public void ClearSlot()
    {
        SkillData = null;
        _skillIcon.sprite = null;
        gameObject.SetActive(false);
    }

    
    public void UpdateCooldown(float cooldownTime, float elapsedTime)
    {
        if (_cooldownImage != null)
        {
            float fillAmount = 1 - (elapsedTime / cooldownTime);
            _cooldownImage.rectTransform.anchorMin = new Vector2(0, 0);
            _cooldownImage.rectTransform.anchorMax = new Vector2(1, fillAmount);
            _cooldownImage.rectTransform.offsetMin = Vector2.zero;
            _cooldownImage.rectTransform.offsetMax = Vector2.zero;
            if (fillAmount <= 0)
            {
                _cooldownImage.gameObject.SetActive(false);
            }
        }
    }
    public void StartCooldown(float cooldownTime)
    {
        _cooldownImage.gameObject.SetActive(true);
        StartCoroutine(CooldownCoroutine(cooldownTime));
    }

    private IEnumerator CooldownCoroutine(float cooldownTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime*1000;
            UpdateCooldown(cooldownTime, elapsedTime);
            yield return null;
        }
        UpdateCooldown(cooldownTime, cooldownTime); // 쿨타임이 끝났을 때 fillAmount를 0으로 설정
    }
}