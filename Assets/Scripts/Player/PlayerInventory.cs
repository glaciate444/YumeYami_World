using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro; // UI操作に必要

public class PlayerInventory : MonoBehaviour{
    public List<ItemData> stockItems = new List<ItemData>();
    private PlayerControls inputActions;

    [Header("所持金設定")]
    public int currentCoins = 0;

    [Header("UI連携")]
    public TMP_Text coinText;

    void Awake(){
        inputActions = new PlayerControls();
        // 例：Vキーなどでストックアイテムを使用するアクション（要InputSystem設定）
        // inputActions.Player.UseItem.performed += context => UseStockItem();

        // ▼【追加】UIの自動検索と初期化
        if (coinText == null){
            GameObject obj = GameObject.FindWithTag("CoinText");
            if (obj != null) coinText = obj.GetComponent<TMP_Text>();
        }
        UpdateUI();
    }

    public void AddItem(ItemData item){
        stockItems.Add(item);
        Debug.Log($"{item.itemName} をストックしました。現在の所持数：{stockItems.Count}");
    }

    public void UseStockItem(){
        if (stockItems.Count > 0){
            ItemData item = stockItems[0]; // 最初のアイテムを使用
            // ここで item.itemType に合わせて回復処理などを実行
            stockItems.RemoveAt(0);
        }
    }

    // ==========================================
    // ▼ ここから下が追加したコイン用の機能 ▼
    // ==========================================

    // コインを拾った時に呼ばれる
    public void AddCoin(int amount){
        currentCoins += amount;
        UpdateUI();
        Debug.Log($"コインをゲット！ 現在: {currentCoins}枚");
    }

    // コインを消費する時に呼ばれる
    public bool SpendCoin(int amount){
        if (currentCoins >= amount){
            currentCoins -= amount;
            UpdateUI();
            return true; // 支払い成功
        }
        return false; // お金が足りない
    }

    // UIのテキストを更新する
    private void UpdateUI(){
        if (coinText != null){
            // "D3" を付けると「005」「099」のように3桁でゼロ埋めされます。
            // 桁数を固定したくない場合は ToString() だけにしてください。
            coinText.text = currentCoins.ToString("D3");
        }
    }
}