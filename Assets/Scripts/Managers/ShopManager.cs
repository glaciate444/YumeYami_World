/* ===================================================
 * スクリプト名 : ShopManager.cs
 * 用途 : キーボード操作前提のショップUI制御と購入処理
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour {
    [Header("商品ラインナップ（5つセットする）")]
    public ShopItemData[] shopItems;

    [Header("UI参照")]
    public Image[] itemIconUI;        // 5つの枠のImage
    public TMP_Text[] itemPriceUI;    // 5つの枠の下にある値段Text
    public TMP_Text messageText;      // 下部のメッセージテキスト
    public TMP_Text currentCoinsText; // 現在の所持コイン表示用
    public RectTransform cursorUI;    // 選択中アイテムを強調するカーソル（または枠）

    private int currentIndex = 0;     // 現在選択している商品のインデックス（0〜4）
    private bool isShopping = true;

    void Start(){
        InitializeShop();
        UpdateUI();
    }

    void Update(){
        if (!isShopping) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // カーソル移動（左右キー）
        if (keyboard.rightArrowKey.wasPressedThisFrame){
            currentIndex = (currentIndex + 1) % shopItems.Length;
            UpdateUI();
        }else if (keyboard.leftArrowKey.wasPressedThisFrame){
            currentIndex = (currentIndex - 1 + shopItems.Length) % shopItems.Length;
            UpdateUI();
        }

        // 購入決定（ZキーまたはEnterキー）
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            TryPurchaseItem();
        }

        // お店を出る（Xキーなど）
        if (keyboard.xKey.wasPressedThisFrame){
            ExitShop();
        }
    }

    private void InitializeShop(){
        // 登録された商品データをUIに反映する
        for (int i = 0; i < itemIconUI.Length; i++){
            if (i < shopItems.Length && shopItems[i] != null){
                itemIconUI[i].sprite = shopItems[i].itemIcon;
                itemIconUI[i].enabled = true;
                itemPriceUI[i].text = shopItems[i].price.ToString();
            }else{
                itemIconUI[i].enabled = false;
                itemPriceUI[i].text = "";
            }
        }
    }

    private void UpdateUI(){
        if (shopItems.Length == 0 || shopItems[currentIndex] == null) return;

        ShopItemData currentItem = shopItems[currentIndex];

        // メッセージと値段の表示更新
        messageText.text = $"{currentItem.itemName}\n{currentItem.description}";

        // GameManagerから現在のコイン総量を取得して表示
        if (GameManager.Instance != null){
            currentCoinsText.text = GameManager.Instance.totalCoins.ToString();
        }

        // カーソル（選択枠）を現在選択しているアイテムの位置へ移動させる
        if (cursorUI != null && itemIconUI[currentIndex] != null){
            cursorUI.position = itemIconUI[currentIndex].transform.position;
        }
    }

    private void TryPurchaseItem(){
        ShopItemData item = shopItems[currentIndex];

        // 1. お金が足りるかチェック
        if (GameManager.Instance.totalCoins < item.price){
            messageText.text = "コインが足りないようだ…";
            return;
        }

        // 2. お金を消費する
        GameManager.Instance.totalCoins -= item.price;

        // 3. 効果を適用する
        ApplyItemEffect(item);

        // 4. セーブしてUIを更新
        GameManager.Instance.SaveGame();
        messageText.text = $"{item.itemName} を購入した！";
        UpdateUI();
    }

    private void ApplyItemEffect(ShopItemData item){
        // 即効性のアイテムはGameManagerの数値を直接書き換える
        switch (item.effectType){
            case ShopItemData.ItemEffectType.Immediate_HpUp:
                GameManager.Instance.currentMaxHp += item.effectValue;
                break;
            case ShopItemData.ItemEffectType.Immediate_SpUp:
                GameManager.Instance.currentMaxSp += item.effectValue;
                break;
            case ShopItemData.ItemEffectType.Immediate_WeaponUp:
                // ※武器レベルの変数がGameManagerにあれば加算する
                // GameManager.Instance.weaponLevel += item.effectValue;
                break;
            case ShopItemData.ItemEffectType.Passive_Inventory:
                // パッシブアイテムの追加処理 ▼▼▼
                if (item.inventoryItemData != null){
                    int idToAdd = item.inventoryItemData.itemId;

                    // まだ持っていない場合のみ GameManager のリストに追加する
                    if (!GameManager.Instance.ownedItemIds.Contains(idToAdd)){
                        GameManager.Instance.ownedItemIds.Add(idToAdd);
                        Debug.Log($"インベントリに {item.itemName} (ID:{idToAdd}) を追加しました！");
                    }else{
                        Debug.Log($"すでに {item.itemName} (ID:{idToAdd}) を持っています！");
                    }
                }
                break;
        }
    }

    private void ExitShop(){
        isShopping = false;
        // ※シーン遷移や、ショップUIを閉じる処理をここに書きます
        // SceneTransitionManager.Instance.LoadScene(GameManager.Instance.returnMapSceneName);
    }
}