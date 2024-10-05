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
        BounceEffect();
    }

    public void BounceEffect()
    {
        StartCoroutine(BounceEffectCoroutine());
    }

    private IEnumerator BounceEffectCoroutine()
    {
        Vector3 originalPosition = transform.position;
        Vector3 targetPosition = originalPosition + new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1.5f), 0);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 바닥에 떨어지는 애니메이션
        elapsed = 0f;
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(targetPosition, originalPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
}