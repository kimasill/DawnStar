using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : BaseController
{
    public Vector3Int SpawnPosition { get; set; }

    public ItemData ItemData { get; set; }

    public int Count { get; set; }

    public Sprite Sprite
    {
        get { return _sprite.sprite; }
        set
        {
            if (_sprite == null)
            {
                _sprite = GetComponent<SpriteRenderer>();
            }
            _sprite.sprite = value;
        }
    }

    protected override void Init()
    {
        base.Init();
    }

    public void MoveToPlayer(Transform playerTransform)
    {
        Managers.Sound.Play("Effect/Pickup", Define.Sound.Effect);
        StartCoroutine(MoveToPlayerCoroutine(playerTransform));
    }

    private IEnumerator MoveToPlayerCoroutine(Transform playerTransform)
    {
        while (Vector3.Distance(transform.position, playerTransform.position) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, Time.deltaTime * 5);
            yield return null;
        }

        SendItemPickupRequestToServer();
        // 아이템 획득 처리
        // 아이템 오브젝트 제거
        Managers.Object.Remove(Id);
    }

    private void SendItemPickupRequestToServer()
    {
        // 서버로 아이템 획득 요청 전송
        C_LootItem lootItemPacket = new C_LootItem
        {
            TemplateId = ItemData.id,
            Count = Count
        };

        Managers.Network.Send(lootItemPacket);
    }

    public void HandleDropItem(PositionInfo position)
    {
        Debug.Log($"HandleDropItem pos:{position}");
        transform.position = Managers.Map.CurrentGrid.CellToWorld(new Vector3Int(position.PosX, position.PosY, 0)) + new Vector3(0.5f, 0.5f);
        Sprite = Resources.Load<Sprite>($"{ItemData.iconPath}");
        gameObject.SetActive(true);
        Managers.Sound.Play("Effect/Drop", Define.Sound.Effect);
        BounceEffect();
    }

    public void BounceEffect()
    {
        StartCoroutine(BounceEffectCoroutine());
    }

    private IEnumerator BounceEffectCoroutine()
    {
        Vector3 originalPosition = transform.position;
        Vector3Int targetCellPosition = Managers.Map.CurrentGrid.WorldToCell(originalPosition) + new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);

        // MapManager를 통해 targetCellPosition이 유효한지 확인
        while (!Managers.Map.CanGo(targetCellPosition))
        {
            targetCellPosition = Managers.Map.CurrentGrid.WorldToCell(originalPosition) + new Vector3Int(Random.Range(-1, 2), Random.Range(-1, 2), 0);
        }

        Vector3 targetPosition = Managers.Map.CurrentGrid.CellToWorld(targetCellPosition) + new Vector3(0.5f, 0.5f, 0);
        Vector3 controlPoint = originalPosition + (targetPosition - originalPosition) / 2 + Vector3.up * 2;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = CalculateBezierPoint(t, originalPosition, controlPoint, targetPosition);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        CellPos = targetCellPosition;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2 * (1-t) * t * p1
        p += tt * p2; // t^2 * p2

        return p;
    }
}