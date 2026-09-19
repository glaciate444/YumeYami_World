/* ===================================================
 * スクリプト名 : PlayerInventory.cs
 * Version : Ver0.03
 * Since : 2026/04/01
 * Update : 2026/09/19
 * 用途 : アイテムインベントリー
 * =================================================== */
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour{
    public List<ItemData> stockItems = new List<ItemData>();
    private PlayerControls inputActions;

    [Header("所持金設定")]
    public int currentCoins = 0;

    void Awake(){
        inputActions = new PlayerControls();
        UpdateUI();
    }

    public void AddItem(ItemData item){
        stockItems.Add(item);
    }

    public void UseStockItem(){
        if (stockItems.Count > 0){
            stockItems.RemoveAt(0);
        }
    }

    public void AddCoin(int amount){
        currentCoins += amount;
        UpdateUI();
    }

    public bool SpendCoin(int amount){
        if (currentCoins >= amount){
            currentCoins -= amount;
            UpdateUI();
            return true; 
        }
        return false; 
    }

    private void UpdateUI(){
        // ▼ HUDManagerへ数値を送るだけ！
        if (HUDManager.Instance != null){
            HUDManager.Instance.UpdateCoin(currentCoins);
        }
    }
}