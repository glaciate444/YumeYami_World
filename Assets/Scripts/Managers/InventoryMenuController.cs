/* ===================================================
 * スクリプト名 : InventoryMenuController.cs
 * Version : Ver0.02
 * Since : 2026/07/15
 * Update : 2026/07/17
 * 用途 : ポーズ画面のカーソル
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // ▼ Image操作用に追加

// ▼ インスペクターで行ごとにスロットを管理するためのクラス（二次元配列の代わり）
[System.Serializable]
public class InventoryRow {
    [Tooltip("この行に含まれるスロット（左から順にセット）")]
    public InventoryItemSlot[] slots;
}

public class InventoryMenuController : MonoBehaviour {
    [Header("UI参照")]
    public RectTransform cursorRect;

    [Header("左側のインベントリ設定")]
    [Tooltip("上から順に行を設定します（0:緑枠, 1:青枠, 2:黄枠など）")]
    public InventoryRow[] inventoryRows;

    [Header("右側の装備先アイコン（画像書き換え用）")]
    public Image equipIconSubAction; // X枠のIcon
    public Image equipIconSpecial;   // C枠のIcon
    public Image equipIconPassiveA;  // パッシブAのIcon
    public Image equipIconPassiveB;  // パッシブBのIcon

    [Header("パッシブ選択用カーソル座標")]
    public RectTransform passiveSlotA_Rect;
    public RectTransform passiveSlotB_Rect;

    [Header("現在の状態（デバッグ用）")]
    public int currentRowIndex = 0;
    public int currentColIndex = 0;
    private bool isActive = false;

    // ▼ パッシブ選択モード用の変数
    private bool isSelectingPassive = false;
    private int selectedPassiveIndex = 0; // 0: PassiveA, 1: PassiveB

    private void OnEnable(){
        isActive = true;
        isSelectingPassive = false;
        currentRowIndex = 0;
        currentColIndex = 0;
        UpdateCursorPosition();
    }

    private void OnDisable() => isActive = false;

    void Update(){
        if (!isActive || inventoryRows == null || inventoryRows.Length == 0) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ▼▼▼ 新規追加：パッシブスロットの選択モード中の処理 ▼▼▼
        if (isSelectingPassive){
            // 左右キーでA枠とB枠を行き来する
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
                selectedPassiveIndex = (selectedPassiveIndex == 0) ? 1 : 0;
                // カーソルをパッシブ枠へ移動
                cursorRect.position = (selectedPassiveIndex == 0) ? passiveSlotA_Rect.position : passiveSlotB_Rect.position;
            }

            // 決定ボタンで装備確定
            if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
                ConfirmEquipPassive();
            }
            // キャンセルボタン（XキーやEsc等）で選択を解除して左のリストに戻る
            else if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
                isSelectingPassive = false;
                UpdateCursorPosition(); // カーソルを左側のリストに戻す
            }
            return; // 選択モード中は、これ以下のリスト移動処理を行わない
        }

        bool moved = false;

        // 現在の行のスロット数を取得（横移動のループや制限に使う）
        int currentRowLength = inventoryRows[currentRowIndex].slots.Length;

        // 【右移動】
        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
            currentColIndex++;
            // 右端を超えたら左端にループ
            if (currentColIndex >= currentRowLength) currentColIndex = 0;
            moved = true;
        }
        // 【左移動】
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame){
            currentColIndex--;
            // 左端を超えたら右端にループ
            if (currentColIndex < 0) currentColIndex = currentRowLength - 1;
            moved = true;
        }
        // 【下移動】
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            currentRowIndex++;
            // 一番下の行を超えたら一番上の行にループ
            if (currentRowIndex >= inventoryRows.Length) currentRowIndex = 0;
            moved = true;
        }
        // 【上移動】
        else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
            currentRowIndex--;
            // 一番上の行を超えたら一番下の行にループ
            if (currentRowIndex < 0) currentRowIndex = inventoryRows.Length - 1;
            moved = true;
        }

        // ▼ 行を上下に移動した際の補正処理（重要）
        // 例：5個ある行の右端から、3個しかない行へ上下移動した時に、存在しない4・5番目を参照してエラーになるのを防ぐ
        if (moved){
            int newRowLength = inventoryRows[currentRowIndex].slots.Length;
            if (currentColIndex >= newRowLength){
                currentColIndex = newRowLength - 1;
            }

            UpdateCursorPosition();
        }

        // ▼ 決定ボタン（例としてZキーやEnterキー）で装備する処理の予約
        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            EquipSelectedItem();
        }
    }

    private void UpdateCursorPosition(){
        if (cursorRect != null && inventoryRows.Length > currentRowIndex && inventoryRows[currentRowIndex].slots.Length > currentColIndex){
            // InventoryItemSlot の Transform（RectTransform）を取得して位置を合わせる
            InventoryItemSlot targetSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
            if (targetSlot != null){
                cursorRect.position = targetSlot.transform.position;
            }
        }
    }

    // ▼▼▼ 新規追加・修正：装備の反映処理 ▼▼▼
    private void EquipSelectedItem(){
        InventoryItemSlot selectedSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
        ItemInventoryData selectedItem = selectedSlot.itemData;

        // 空枠（アイテムデータが入っていない）を選んだ場合は何もしない
        if (selectedItem == null) return;

        // プレイヤーのスクリプトを取得（シーンに確実に存在するものとする）
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        PlayerShoot ps = FindFirstObjectByType<PlayerShoot>();

        switch (selectedItem.category){
            case ItemCategory.SubAction: // 緑枠（X）
                if (pc != null) pc.currentSubActionEquip = selectedItem;
                if (equipIconSubAction != null) equipIconSubAction.sprite = selectedItem.icon;
                Debug.Log($"{selectedItem.itemName} をサブアクション(X)に装備しました。");
                break;

            case ItemCategory.Special: // 青枠（C）
                if (ps != null) ps.currentSpecialEquip = selectedItem;
                if (equipIconSpecial != null) equipIconSpecial.sprite = selectedItem.icon;
                Debug.Log($"{selectedItem.itemName} をスペシャル(C)に装備しました。");
                break;

            case ItemCategory.Passive: // 黄色枠
                // すぐに装備せず、右側のパッシブ枠選択モードに移行する
                isSelectingPassive = true;
                selectedPassiveIndex = 0;
                cursorRect.position = passiveSlotA_Rect.position; // カーソルを右側のA枠に飛ばす
                Debug.Log("どちらのパッシブ枠に装備するか選択してください。");
                break;
        }
    }

    // パッシブ枠のA・Bどちらに装備するか確定した時の処理
    private void ConfirmEquipPassive(){
        InventoryItemSlot selectedSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
        ItemInventoryData selectedItem = selectedSlot.itemData;

        // プレイヤーへの反映（パッシブ用変数がPlayerControllerにある想定）
        // ※ PlayerController側に public ItemInventoryData currentPassiveA; 等を追加してください
        PlayerController pc = FindFirstObjectByType<PlayerController>();

        if (selectedPassiveIndex == 0){// Passive A
            // if (pc != null) pc.currentPassiveA = selectedItem; 
            if (equipIconPassiveA != null) equipIconPassiveA.sprite = selectedItem.icon;
            Debug.Log($"{selectedItem.itemName} を パッシブA に装備しました。");
        }else{ // Passive B
            // if (pc != null) pc.currentPassiveB = selectedItem;
            if (equipIconPassiveB != null) equipIconPassiveB.sprite = selectedItem.icon;
            Debug.Log($"{selectedItem.itemName} を パッシブB に装備しました。");
        }

        // 選択モードを終了し、カーソルを左のリストに戻す
        isSelectingPassive = false;
        UpdateCursorPosition();
    }
}