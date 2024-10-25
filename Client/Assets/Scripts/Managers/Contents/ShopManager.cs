using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class ShopManager
{
    public Dictionary<int, Shop> Shops = new Dictionary<int, Shop>();

    public void InitializeShop(int mapId)
    {
        Dictionary<int, ShopData> shopDict = Managers.Data.ShopDict;
        if (shopDict == null)
        {
            Debug.Log("ShopDict is null");
            return;
        }
        foreach (var shopData in shopDict.Values)
        {
            if (shopData.mapId == mapId)
            {
                Shop shop = new Shop();
                shop.name = shopData.name;
                shop.shopId = shopData.id;
                Shops[shop.shopId] = shop;

                C_RequestShop requestShopPacket = new C_RequestShop();
                requestShopPacket.ShopId = shop.shopId;
                Managers.Network.Send(requestShopPacket); ;
            }
        }
    }
    public void Clear()
    {
        Shops.Clear();
    }
}