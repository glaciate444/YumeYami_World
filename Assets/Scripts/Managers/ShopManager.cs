/* ===================================================
 * スクリプト名 : ShopManager.cs
 * 用途 : キーボード操作前提のショップUI制御と購入処理
 * 拡張 : 売り切れ判定と、購入回数による価格上昇（ドーピング）を実装
 * =================================================== */
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour {
    [Header("商品ラインナップ（5つセットする）")]
    public ShopItemData[] shopItems;

    // ▼▼▼ 新規追加：価格変動の設定 ▼▼▼
    [Header("ドーピング価格設定（最大6回）")]
    [Tooltip("左から順に1回目, 2回目...の価格になります")]
    public int[] dopingPrices = { 200, 500, 1000, 1500, 2000, 3000 };
    private int maxDopingCount = 6;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("UI参照")]
    public Image[] itemIconUI;
    public TMP_Text[] itemPriceUI;
    public TMP_Text messageText;
    public TMP_Text currentCoinsText;
    public RectTransform cursorUI;

    private int currentIndex = 0;
    private bool isShopping = true;

    void Start(){
        InitializeShop();
        UpdateUI();
    }

    void Update(){
        if (!isShopping) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rightArrowKey.wasPressedThisFrame){
            currentIndex = (currentIndex + 1) % shopItems.Length;
            UpdateUI();
        }else if (keyboard.leftArrowKey.wasPressedThisFrame){
            currentIndex = (currentIndex - 1 + shopItems.Length) % shopItems.Length;
            UpdateUI();
        }

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            TryPurchaseItem();
        }

        if (keyboard.xKey.wasPressedThisFrame){
            ExitShop();
        }
    }

    private void InitializeShop(){
        for (int i = 0; i < itemIconUI.Length; i++){
            if (i < shopItems.Length && shopItems[i] != null){
                itemIconUI[i].sprite = shopItems[i].itemIcon;
                itemIconUI[i].enabled = true;
            }else{
                itemIconUI[i].enabled = false;
                itemPriceUI[i].text = "";
            }
        }
    }

    private void UpdateUI(){
        if (shopItems.Length == 0 || shopItems[currentIndex] == null) return;
        ShopItemData currentItem = shopItems[currentIndex];

        if (GameManager.Instance != null){
            currentCoinsText.text = GameManager.Instance.totalCoins.ToString();
        }

        // ▼ 全アイコンの価格表示と「売り切れ時の暗転」を更新
        for (int i = 0; i < itemIconUI.Length; i++){
            if (i < shopItems.Length && shopItems[i] != null){
                if (IsSoldOut(shopItems[i])){
                    itemIconUI[i].color = new Color(0.3f, 0.3f, 0.3f, 1f); // 売り切れは画像を暗くする
                    itemPriceUI[i].text = "SOLD OUT";
                }else{
                    itemIconUI[i].color = Color.white; // 通常の色
                    itemPriceUI[i].text = GetCurrentPrice(shopItems[i]).ToString();
                }
            }
        }

        // ▼ メッセージの更新
        if (IsSoldOut(currentItem)){
            messageText.text = $"{currentItem.itemName}\n【売り切れ】";
        }else{
            messageText.text = $"{currentItem.itemName}\n{currentItem.description}";
        }

        if (cursorUI != null && itemIconUI[currentIndex] != null){
            cursorUI.position = itemIconUI[currentIndex].transform.position;
        }
    }

    // ▼▼▼ 新規追加：売り切れかどうかを判定するメソッド ▼▼▼
    private bool IsSoldOut(ShopItemData item)
    {
        if (GameManager.Instance == null) return false;

        // パッシブアイテム：すでにIDを持っているか？
        if (item.effectType == ShopItemData.ItemEffectType.Passive_Inventory){
            if (item.inventoryItemData != null && GameManager.Instance.ownedItemIds.Contains(item.inventoryItemData.itemId)){
                return true;
            }
        }
        // HPドーピング：最大回数（6回）買ったか？
        else if (item.effectType == ShopItemData.ItemEffectType.Immediate_HpUp){
            return GameManager.Instance.hpUpPurchaseCount >= maxDopingCount;
        }
        // SPドーピング：最大回数（6回）買ったか？
        else if (item.effectType == ShopItemData.ItemEffectType.Immediate_SpUp){
            return GameManager.Instance.spUpPurchaseCount >= maxDopingCount;
        }

        return false;
    }

    // ▼▼▼ 新規追加：現在の価格を計算するメソッド ▼▼▼
    private int GetCurrentPrice(ShopItemData item){
        if (GameManager.Instance == null) return item.price;

        if (item.effectType == ShopItemData.ItemEffectType.Immediate_HpUp){
            int count = GameManager.Instance.hpUpPurchaseCount;
            if (count < dopingPrices.Length) return dopingPrices[count];
        }else if (item.effectType == ShopItemData.ItemEffectType.Immediate_SpUp){
            int count = GameManager.Instance.spUpPurchaseCount;
            if (count < dopingPrices.Length) return dopingPrices[count];
        }

        return item.price; // パッシブアイテム等は設定された固定価格
    }

    private void TryPurchaseItem(){
        ShopItemData item = shopItems[currentIndex];

        // 1. 売り切れチェック
        if (IsSoldOut(item)){
            messageText.text = "その商品はすでに売り切れだ！";
            return;
        }

        // 2. お金チェック
        int price = GetCurrentPrice(item);
        if (GameManager.Instance.totalCoins < price){
            messageText.text = "コインが足りないようだ…";
            return;
        }

        // 3. 購入処理
        GameManager.Instance.totalCoins -= price;
        ApplyItemEffect(item);
        GameManager.Instance.SaveGame();

        messageText.text = $"{item.itemName} を購入した！";
        UpdateUI();
    }

    private void ApplyItemEffect(ShopItemData item){
        switch (item.effectType){
            case ShopItemData.ItemEffectType.Immediate_HpUp:
                GameManager.Instance.currentMaxHp += item.effectValue;
                GameManager.Instance.hpUpPurchaseCount++; // ▼ 購入回数を+1
                break;
            case ShopItemData.ItemEffectType.Immediate_SpUp:
                GameManager.Instance.currentMaxSp += item.effectValue;
                GameManager.Instance.spUpPurchaseCount++; // ▼ 購入回数を+1
                break;
            case ShopItemData.ItemEffectType.Immediate_WeaponUp:
                break;
            case ShopItemData.ItemEffectType.Passive_Inventory:
                if (item.inventoryItemData != null){
                    int idToAdd = item.inventoryItemData.itemId;
                    if (!GameManager.Instance.ownedItemIds.Contains(idToAdd)){
                        GameManager.Instance.ownedItemIds.Add(idToAdd);
                    }
                }
                break;
        }
    }

    private void ExitShop(){
        isShopping = false;
        // ここを実装：GameManagerの記憶から帰り道を探す ▼▼▼
        string returnScene = "WorldMapScene"; // 記憶がなかった場合の予備

        if (GameManager.Instance != null && !string.IsNullOrEmpty(GameManager.Instance.returnMapSceneName)){
            returnScene = GameManager.Instance.returnMapSceneName; // 例：MapSelectScene_Level_1 が入る
        }

        // 暗転フェードで元のマップへ帰還
        if (SceneTransitionManager.Instance != null){
            SceneTransitionManager.Instance.LoadScene(returnScene, TransitionType.Fade);
        }else{
            SceneManager.LoadScene(returnScene);
        }
    }
}