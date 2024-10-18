using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void MoveToPosition(Vector3 targetPosition)
    {
        StartCoroutine(SmoothMove(targetPosition));
    }

    private IEnumerator SmoothMove(Vector3 targetPosition)
    {
        while ((transform.position - targetPosition).sqrMagnitude > 0.01f)
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
            transform.position = smoothedPosition;
            yield return null;
        }
        transform.position = targetPosition;
    }

    public IEnumerator ResetCameraAndTarget(float time)
    {
        yield return new WaitForSeconds(time);
        // 카메라를 원래 위치로 되돌리기
        MyPlayerController myPlayer = Managers.Object.MyPlayer;
        Vector2Int playerPos = (Vector2Int)myPlayer.CellPos;
        MoveToPosition(new Vector3(playerPos.x, playerPos.y, -10));

        // 타겟을 플레이어로 설정하기
        if (myPlayer.gameObject.activeSelf == false)
        {
            myPlayer.gameObject.SetActive(true);
        }
        SetTarget(Managers.Object.MyPlayer.transform);
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, -10);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}