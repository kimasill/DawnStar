using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
	protected Dictionary<Type, UnityEngine.Object[]> _objects = new Dictionary<Type, UnityEngine.Object[]>();
	public abstract void Init();
	protected bool _isFading = false;
	private void Awake()
	{
		Init();
	}

	protected void Bind<T>(Type type) where T : UnityEngine.Object
	{
		string[] names = Enum.GetNames(type);
		UnityEngine.Object[] objects = new UnityEngine.Object[names.Length];
		_objects.Add(typeof(T), objects);

		for (int i = 0; i < names.Length; i++)
		{
			if (typeof(T) == typeof(GameObject))
				objects[i] = Util.FindChild(gameObject, names[i], true);
			else
				objects[i] = Util.FindChild<T>(gameObject, names[i], true);

			if (objects[i] == null)
				Debug.Log($"Failed to bind({names[i]})");
		}
	}

	protected T Get<T>(int idx) where T : UnityEngine.Object
	{
		UnityEngine.Object[] objects = null;
		if (_objects.TryGetValue(typeof(T), out objects) == false)
			return null;

		return objects[idx] as T;
	}

	protected GameObject GetObject(int idx) { return Get<GameObject>(idx); }
	protected Text GetText(int idx) { return Get<Text>(idx); }
	protected Button GetButton(int idx) { return Get<Button>(idx); }
	protected Image GetImage(int idx) { return Get<Image>(idx); }
	protected TMP_Text GetTextMeshPro(int idx) { return Get<TMP_Text>(idx); }

	public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_EventHandler evt = Util.GetOrAddComponent<UI_EventHandler>(go);

		switch (type)
		{
			case Define.UIEvent.Click:
				evt.OnClickHandler -= action;
				evt.OnClickHandler += action;
				break;
			case Define.UIEvent.RightClick:
                evt.OnRightClickHandler -= action;
                evt.OnRightClickHandler += action;
                break;
            case Define.UIEvent.Drag:
				evt.OnDragHandler -= action;
				evt.OnDragHandler += action;
				break;
			case Define.UIEvent.MouseOver:
                evt.OnMouseOverHandler -= action;
                evt.OnMouseOverHandler += action;
                break;
            case Define.UIEvent.MouseOut:
                evt.OnMouseOutHandler -= action;
                evt.OnMouseOutHandler += action;
                break;
        }
	}
    protected virtual IEnumerator FadeIn(Image image, float fadingTime)
    {
        _isFading = true; // 페이드 인 시작
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime / fadingTime;
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }
        _isFading = false; // 페이드 인 종료
    }
    protected virtual IEnumerator FadeOut(Image image, float fadingTime)
    {
        _isFading = true; // 페이드 아웃 시작
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / fadingTime;
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            yield return null;
        }
        _isFading = false; // 페이드 아웃 종료
    }
	protected virtual IEnumerator FadeInAll(GameObject gameObject, float fadingTime)
	{
		_isFading = true;
        float alpha = 0;
        while (alpha < 1)
        {
            alpha += Time.deltaTime / fadingTime;
            gameObject.GetComponent<CanvasGroup>().alpha = alpha;
            yield return null;
        }
        _isFading = false;
    }
    protected virtual IEnumerator FadeOutAll(GameObject gameObject, float fadingTime)
    {
        _isFading = true;
        float alpha = 1;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime / fadingTime;
            gameObject.GetComponent<CanvasGroup>().alpha = alpha;
            yield return null;
        }
        _isFading = false;
    }

}
