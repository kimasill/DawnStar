using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using TMPro;
using UnityEngine;

public class CabinController : BaseController
{
    private bool _isOpened = false;
    public string Name { get; private set; }
    public int TemplateId { get; private set; } = 2;
    [SerializeField]
    public GameObject _cabinTop;

    [SerializeField]
    public GameObject _door;
    DoorController _doorController;

    IEnumerator _routine;
    protected override void Init()
    {
        _doorController = _door.GetComponent<DoorController>();
        _doorController._openAction = OpenCabinTop;
        _doorController._closeAction = CloseCabinTop;
        TemplateId = int.Parse(gameObject.name.Split('_')[1]);
        Animator = _cabinTop.GetComponent<Animator>();
        if (Animator != null)
        {
            Animator.speed = 0;
            Animator.Play($"CLOSE{TemplateId}", 0, 1.0f);
        }
    }
    private void OpenCabinTop()
    {
        if (_isOpened)
            return;        
        _isOpened = true;
        StartCoroutine(CoOpenTop());
    }
    private void CloseCabinTop()
    {
        if (!_isOpened)
            return;
        _isOpened = false;
        StartCoroutine(CoCloseTop());
    }

    private IEnumerator CoOpenTop()
    {
        Animator.speed = 1;
        Animator.Play($"OPEN{TemplateId}"); // OPEN 애니메이션 재생
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f); // 애니메이션 종료 대기
        Animator.speed = 0;
        Animator.Play($"CLOSE{TemplateId}", 0, 0); // CLOSE 애니메이션 첫 프레임으로 고정
    }

    private IEnumerator CoCloseTop()
    {
        Animator.speed = 1;
        Animator.Play($"OPEN{TemplateId}"); // OPEN 애니메이션 재생
        yield return new WaitUntil(() => Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f); // 애니메이션 종료 대기
        Animator.speed = 0;
        Animator.Play($"CLOSE{TemplateId}", 0, 0); // CLOSE 애니메이션 첫 프레임으로 고정
    }

    protected override void UpdateAnimation()
    {
    }
}