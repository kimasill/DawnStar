using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Respawn : UI_Popup
{
    private S_Respawn _respawnPacket;

    enum Buttons
    {
        SpotButton,
        CancelBtn
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.SpotButton).gameObject.BindEvent(OnClickSpotButton);
        GetButton((int)Buttons.CancelBtn).gameObject.BindEvent(OnClickRepeatButton);
    }

    public void SetRespawnPacket(S_Respawn respawnPacket)
    {
        _respawnPacket = respawnPacket;
    }

    private void OnClickSpotButton(PointerEventData evt)
    {
        // 부활 패킷 전송
        C_Respawn respawnPacket = new C_Respawn();
        respawnPacket.RespawnType = RespawnType.Spot;
        Managers.Network.Send(respawnPacket);

        // 부활 UI 닫기 및 입력 차단 해제
        ClosePopupUI();        
    }

    private void OnClickRepeatButton(PointerEventData evt)
    {
        C_Respawn respawnPacket = new C_Respawn();
        respawnPacket.RespawnType = RespawnType.Repeat;
        Managers.Network.Send(respawnPacket);

        // 부활 UI 닫기 및 입력 차단 해제
        ClosePopupUI();
    }
}