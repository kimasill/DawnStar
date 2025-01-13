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

    public bool IsMouseover = false;
    public abstract void Init();
	protected bool _isFading = false;
    protected Vector3 _originalPosition;
    protected Transform _originalParent;
    protected int _originalSiblingIndex;
    protected bool _isDragging = false;
    protected bool _isDraggingOver = false;
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
            case Define.UIEvent.BeginDrag:
                evt.OnBeginDragHandler -= action;
                evt.OnBeginDragHandler += action;
                break;
            case Define.UIEvent.EndDrag:
                evt.OnEndDragHandler -= action;
                evt.OnEndDragHandler += action;
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
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        IsMouseover = true;
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        IsMouseover = false;
    }
    public virtual void OnScroll(float delta)
    {
        if (IsMouseover)
        {
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {       

        gameObject.GetOrAddComponent<CanvasGroup>().blocksRaycasts = false;
        _originalPosition = transform.position;
        _originalSiblingIndex = transform.GetSiblingIndex();
        _originalParent = transform.parent;
        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        transform.SetParent(gameScene.transform);
        transform.SetAsLastSibling();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        Vector3 newPosition = eventData.position;

        UI_GameScene gameScene = Managers.UI.SceneUI as UI_GameScene;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(gameScene.transform as RectTransform, eventData.position, eventData.pressEventCamera, out newPosition);
        transform.position = newPosition;

        // 화면 밖으로 나가지 않게 제한
        Vector3[] canvasCorners = new Vector3[4];
        (gameScene.transform as RectTransform).GetWorldCorners(canvasCorners);
        Vector3[] itemCorners = new Vector3[4];
        (transform as RectTransform).GetWorldCorners(itemCorners);

        if (itemCorners[0].x < canvasCorners[0].x)
            transform.position += Vector3.right * (canvasCorners[0].x - itemCorners[0].x);
        if (itemCorners[2].x > canvasCorners[2].x)
            transform.position += Vector3.left * (itemCorners[2].x - canvasCorners[2].x);
        if (itemCorners[0].y < canvasCorners[0].y)
            transform.position += Vector3.up * (canvasCorners[0].y - itemCorners[0].y);
        if (itemCorners[2].y > canvasCorners[2].y)
            transform.position += Vector3.down * (itemCorners[2].y - canvasCorners[2].y);
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        gameObject.GetOrAddComponent<CanvasGroup>().blocksRaycasts = true;        
        transform.SetParent(_originalParent);
    }
}
