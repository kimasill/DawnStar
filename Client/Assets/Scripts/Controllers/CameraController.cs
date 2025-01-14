using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

public class CameraController : MonoBehaviour
{
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothTime = 0.075f;
    public float zoomLevel = 2f;
    private Vector3 velocity = Vector3.zero;
    private Transform target;

    public float minSmoothTime = 0.1f; // 최소 이동 속도
    public float maxSmoothTime = 0.5f; // 최대 이동 속도
    public float distanceThreshold = 5f; // 속도 조절을 시작할 거리

    public int assetsPPU = 200; // 픽셀 퍼 유닛
    public int refResolutionX = 1920; // 참조 해상도 X
    public int refResolutionY = 1080; // 참조 해상도 Y

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    public void SetPixelPerfect(int resolutionX = 0, int resolutionY = 0)
    {
        PixelPerfectCamera pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();
        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = Camera.main.gameObject.AddComponent<PixelPerfectCamera>();
        }
        pixelPerfectCamera.assetsPPU = assetsPPU;
        if(resolutionX != 0 && resolutionY != 0)
        {
            pixelPerfectCamera.refResolutionX = resolutionX;
            pixelPerfectCamera.refResolutionY = resolutionY;
        }
        else
        {
            pixelPerfectCamera.refResolutionX = refResolutionX;
            pixelPerfectCamera.refResolutionY = refResolutionY;
        }
        pixelPerfectCamera.upscaleRT = true;
        pixelPerfectCamera.pixelSnapping = true;
        pixelPerfectCamera.stretchFill = true;
        pixelPerfectCamera.refResolutionX = (int)(refResolutionX / zoomLevel);
        pixelPerfectCamera.refResolutionY = (int)(refResolutionY / zoomLevel);
    }
    public IEnumerator MoveToPosition(Transform targetTransform)
    {
        target = null;
        Vector3 targetPosition = new Vector3(targetTransform.position.x, targetTransform.position.y, -10);
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f) // 조건 수정
        {
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothTime);
            transform.position = smoothedPosition;
            yield return null;
        }
        transform.position = targetPosition;
    }

    private IEnumerator SmoothMove(Transform targetTransform)
    {
        target = null;
        Vector3 targetPosition = new Vector3(targetTransform.position.x, targetTransform.position.y, -10);
        // 카메라 이동
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            // 대상과의 거리에 따라 smoothTime을 동적으로 조절
            float distance = Vector3.Distance(transform.position, targetPosition);
            float smoothTime = Mathf.Lerp(minSmoothTime, maxSmoothTime, Mathf.Clamp01(distance / distanceThreshold));

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothTime);
            transform.position = smoothedPosition;
            yield return null;
        }

        transform.position = targetTransform.position; // while 문 종료 후 위치를 정확히 일치시킴
    }

    public IEnumerator ResetCameraAndTarget(float time)
    {
        yield return new WaitForSeconds(time);
        // 카메라를 원래 위치로 되돌리기
        MyPlayerController myPlayer = Managers.Object.MyPlayer;        
        MoveToPosition(myPlayer.transform);

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