/* ===================================================
 * スクリプト名 : InventoryMenuController.cs
 * Version : Ver0.03
 * Since : 2026/07/15
 * Update : 2026/07/17
 * 用途 : ポーズ画面のカーソル
 * 更新 : 装備再計算
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

    [Header("コース退出UI用カーソル座標")]
    public RectTransform stageExitRect; // 「STAGE EXIT」の座標
    public RectTransform dialogYesRect; // ダイアログの「はい」の座標
    public RectTransform dialogNoRect;  // ダイアログの「いいえ」の座標

    [Header("現在の状態（デバッグ用）")]
    public int currentRowIndex = 0;
    public int currentColIndex = 0;
    private bool isActive = false;

    // ▼ パッシブ選択モード用の変数
    private bool isSelectingPassive = false;
    private int selectedPassiveIndex = 0; // 0: PassiveA, 1: PassiveB

    private bool isFocusingStageExit = false; // カーソルが「STAGE EXIT」にあるか
    private bool isExitDialogOpen = false;    // ダイアログが開いているか
    private bool isYesSelected = false;       // ダイアログ内で「はい」を選んでいるか

    private void OnEnable(){
        isActive = true;
        isSelectingPassive = false;
        currentRowIndex = 0;
        currentColIndex = 0;
        // ▼▼▼ 新規追加：メニューを開いた瞬間、全マスの表示を最新レベルに更新する ▼▼▼
        if (inventoryRows != null){
            foreach (var row in inventoryRows){
                if (row.slots != null){
                    foreach (var slot in row.slots){
                        if (slot != null){
                            slot.UpdateSlotUI(); // ここで星や透明化を自動処理
                        }
                    }
                }
            }
        }

        UpdateCursorPosition();
    }

    private void OnDisable() => isActive = false;

    void Update(){
        if (!isActive || inventoryRows == null || inventoryRows.Length == 0) return;
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // ====================================================
        // 状態1：コース退出ダイアログが開いている時の操作
        // ====================================================
        if (isExitDialogOpen){
            // 左右で「はい / いいえ」の切り替え
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
                isYesSelected = !isYesSelected;
                cursorRect.position = isYesSelected ? dialogYesRect.position : dialogNoRect.position;
            }

            // 決定ボタン
            if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
                if (isYesSelected){
                    PauseManager.Instance.ConfirmExitCourse(); // 退出実行
                }else{
                    CancelExitDialog(); // 退出キャンセル
                }
            }
            // キャンセルボタン
            else if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
                CancelExitDialog();
            }
            return; // ダイアログ操作中は他の処理をしない
        }

        // ====================================================
        // 状態2：パッシブ枠を選択している時の操作
        // ====================================================
        if (isSelectingPassive){
            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame ||
                keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
                selectedPassiveIndex = (selectedPassiveIndex == 0) ? 1 : 0;
                cursorRect.position = (selectedPassiveIndex == 0) ? passiveSlotA_Rect.position : passiveSlotB_Rect.position;
            }

            if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
                ConfirmEquipPassive();
            }else if (keyboard.xKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame){
                isSelectingPassive = false;
                UpdateCursorPosition();
            }
            return;
        }

        // ====================================================
        // 状態3：「STAGE EXIT」にカーソルが合っている時の操作
        // ====================================================
        if (isFocusingStageExit){
            // 上を押すとインベントリの一番下の行に戻る
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
                isFocusingStageExit = false;
                currentRowIndex = inventoryRows.Length - 1;
                UpdateCursorPosition();
            }
            // 下を押すとループしてインベントリの一番上に戻る
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
                isFocusingStageExit = false;
                currentRowIndex = 0;
                UpdateCursorPosition();
            }

            // 決定ボタンでダイアログを開く
            if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
                isExitDialogOpen = true;
                isYesSelected = false; // 誤爆防止のため、最初は「いいえ」に合わせておく
                PauseManager.Instance.OpenExitDialog();
                cursorRect.position = dialogNoRect.position;
            }
            return;
        }

        // ====================================================
        // 状態4：通常のインベントリ移動操作
        // ====================================================
        bool moved = false;
        int currentRowLength = inventoryRows[currentRowIndex].slots.Length;

        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame){
            currentColIndex++;
            if (currentColIndex >= currentRowLength) currentColIndex = 0;
            moved = true;
        }else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame){
            currentColIndex--;
            if (currentColIndex < 0) currentColIndex = currentRowLength - 1;
            moved = true;
        }else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame){
            currentRowIndex++;
            // 一番下の行から更に下に行こうとしたら「STAGE EXIT」へフォーカスを移す
            if (currentRowIndex >= inventoryRows.Length){
                isFocusingStageExit = true;
                cursorRect.position = stageExitRect.position;
            }else{
                moved = true;
            }
        }else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame){
            currentRowIndex--;
            // 一番上の行から更に上に行こうとしたら「STAGE EXIT」へフォーカスを移す
            if (currentRowIndex < 0)
            {
                isFocusingStageExit = true;
                cursorRect.position = stageExitRect.position;
            }else{
                moved = true;
            }
        }

        if (moved){
            int newRowLength = inventoryRows[currentRowIndex].slots.Length;
            if (currentColIndex >= newRowLength) currentColIndex = newRowLength - 1;
            UpdateCursorPosition();
        }

        if (keyboard.zKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame){
            EquipSelectedItem();
        }
    }

    // ダイアログをキャンセルした時の共通処理
    private void CancelExitDialog(){
        isExitDialogOpen = false;
        PauseManager.Instance.CloseExitDialog();
        cursorRect.position = stageExitRect.position; // カーソルを「STAGE EXIT」に戻す
    }

    private void UpdateCursorPosition(){
        if (cursorRect != null && inventoryRows.Length > currentRowIndex && inventoryRows[currentRowIndex].slots.Length > currentColIndex){
            InventoryItemSlot targetSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
            if (targetSlot != null){
                cursorRect.position = targetSlot.transform.position;
            }
        }
    }

    private void EquipSelectedItem(){
        InventoryItemSlot selectedSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
        ItemInventoryData selectedItem = selectedSlot.itemData;
        if (selectedItem == null) return;

        // ▼▼▼ 修正：アクションコマンドは強制許可、それ以外はレベル0ならブロック ▼▼▼
        int level = 0;
        if (GameManager.Instance != null){
            level = GameManager.Instance.GetItemLevel(selectedItem.itemId);
        }

        // 特例1：アクションコマンド（緑枠）は無条件で装備許可
        if (selectedItem.category == ItemCategory.SubAction && level <= 0){
            level = 1;
        }

        // 特例2：初期スペシャル技（例：ID21）も無条件で装備許可
        if (selectedItem.itemId == 21 && level <= 0){
            level = 1;
        }

        // レベル0のまま（＝本当に持っていないパッシブ等）ならここで処理を強制終了
        if (level <= 0){
            return;
        }

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        PlayerShoot ps = FindFirstObjectByType<PlayerShoot>();

        switch (selectedItem.category){
            case ItemCategory.SubAction:
                if (pc != null) pc.currentSubActionEquip = selectedItem;
                if (GameManager.Instance != null) GameManager.Instance.currentEquipSubAction = selectedItem;
                if (equipIconSubAction != null) equipIconSubAction.sprite = selectedItem.icon;
                break;
            case ItemCategory.Special:
                if (ps != null) ps.currentSpecialEquip = selectedItem;
                if (GameManager.Instance != null) GameManager.Instance.currentEquipSpecial = selectedItem;
                if (equipIconSpecial != null) equipIconSpecial.sprite = selectedItem.icon;
                break;
            case ItemCategory.Passive:
                isSelectingPassive = true;
                selectedPassiveIndex = 0;
                cursorRect.position = passiveSlotA_Rect.position;
                break;
        }

        EquipHUD hud = FindFirstObjectByType<EquipHUD>();
        if (hud != null) hud.UpdateHUD();
    }

    private void ConfirmEquipPassive(){
        InventoryItemSlot selectedSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
        ItemInventoryData selectedItem = selectedSlot.itemData;

        PlayerController pc = FindFirstObjectByType<PlayerController>();

        // ▼▼▼ 1. 重複装備の防止処理 ▼▼▼
        if (pc != null){
            // A枠に装備しようとした時、B枠に同じものがあればB枠を空にする
            if (selectedPassiveIndex == 0 && pc.equipPassiveB == selectedItem){
                pc.equipPassiveB = null;
                if (equipIconPassiveB != null) equipIconPassiveB.color = new Color(1, 1, 1, 0); // 画像を透明にして隠す
            }
            // B枠に装備しようとした時、A枠に同じものがあればA枠を空にする
            else if (selectedPassiveIndex == 1 && pc.equipPassiveA == selectedItem){
                pc.equipPassiveA = null;
                if (equipIconPassiveA != null) equipIconPassiveA.color = new Color(1, 1, 1, 0);
            }
        }

        // ▼▼▼ 2. 選択した枠への装備処理 ▼▼▼
        if (selectedPassiveIndex == 0){
            if (equipIconPassiveA != null){
                equipIconPassiveA.sprite = selectedItem.icon;
                equipIconPassiveA.color = new Color(1, 1, 1, 1); // 不透明にして表示
            }
            if (pc != null) pc.equipPassiveA = selectedItem;
            if (GameManager.Instance != null) GameManager.Instance.currentEquipPassiveA = selectedItem; // ▼ 追加
        }
        else{
            if (equipIconPassiveB != null)
            {
                equipIconPassiveB.sprite = selectedItem.icon;
                equipIconPassiveB.color = new Color(1, 1, 1, 1);
            }
            if (pc != null) pc.equipPassiveB = selectedItem;
            if (GameManager.Instance != null) GameManager.Instance.currentEquipPassiveB = selectedItem;
        }

        // ▼▼▼ 3. ステータスの再計算を実行 ▼▼▼
        if (pc != null) pc.ApplyPassiveEffects();

        // ▼▼▼ 4. PauseManagerを呼んで、画面右下の数字を即座に最新化する ▼▼▼
        if (PauseManager.Instance != null) PauseManager.Instance.UpdatePersonalData();

        isSelectingPassive = false;
        UpdateCursorPosition();
    }
    // 持っていないアイテムを透明にして隠す処理
    private void UpdateInventoryVisibility(){
        if (GameManager.Instance == null) return;

        foreach (var row in inventoryRows){
            foreach (var slot in row.slots){
                if (slot != null && slot.itemData != null){
                    // GameManagerに「このアイテムのレベルはいくつか（持っているか）？」を聞く
                    int level = GameManager.Instance.GetItemLevel(slot.itemData.itemId);
                    Image iconImage = slot.GetComponent<Image>();

                    if (iconImage != null){
                        if (level > 0){
                            // 持っている（Lv1以上）：不透明にして表示する
                            iconImage.color = new Color(1f, 1f, 1f, 1f);
                        }else{
                            // 持っていない（Lv0）：透明にして完全に隠す
                            iconImage.color = new Color(1f, 1f, 1f, 0f);
                            // ※もし未所持を「黒いシルエット」にしたい場合は new Color(0f, 0f, 0f, 0.7f) などにします
                        }
                    }
                }
            }
        }
    }
}