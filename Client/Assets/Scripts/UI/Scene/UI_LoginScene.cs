using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    enum GameObjects
    {
        AccountName,
        Password
    }

    enum Buttons
    { 
        CreateBtn,
        LoginBtn
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));    
        Bind<GameObject>(typeof(GameObjects));

        GetButton((int)Buttons.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
        GetButton((int)Buttons.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
    }

    public void OnClickCreateButton(PointerEventData evt)
    {
        string account = Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text;
        string password = Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text;

        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<CreateAccountPacketRes>("account/create", packet, (res) =>
        {
            Debug.Log($"Create Success : {res.CreateSuccess}");
            Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text = "";
        });

        // 버튼 활성화 및 로딩 스피너 비활성화
        //GetButton((int)Buttons.CreateBtn).interactable = true;        
    }

    public void OnClickLoginButton(PointerEventData evt)
    {
        string account = Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text;
        string password = Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text;

        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };

        Managers.Web.SendPostRequest<LoginAccountPacketRes>("account/login", packet, (res) =>
        {
            Debug.Log($"Create Success : {res.LoginSuccess}");
            Get<GameObject>((int)GameObjects.AccountName).GetComponent<InputField>().text = "";
            Get<GameObject>((int)GameObjects.Password).GetComponent<InputField>().text = "";

            if (res.LoginSuccess)
            {
                Managers.Network.AccountId = res.AccountId;
                Managers.Network.Token = res.Token;
                UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                popup.SetServers(res.ServerList);
            } 
        });

        //GetButton((int)Buttons.LoginBtn).interactable = true;
    }
}
