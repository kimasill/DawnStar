using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : BaseController
{
    public Vector3Int SpawnPosition { get; set; }

    private SpriteRenderer _spriteRenderer;

    public ItemData ItemData { get; set; }

    public int Count { get; set; }

    public Sprite Sprite
    {
        get { return _spriteRenderer.sprite; }
        set { _spriteRenderer.sprite = value; }
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
        // 서버에 아이템 획득 요청을 보냄
        C_RootItem rootItemPacket = new C_RootItem
        {
            TemplateId = ItemData.id,
            Count = Count
        };

        Managers.Network.Send(rootItemPacket);
    }
}