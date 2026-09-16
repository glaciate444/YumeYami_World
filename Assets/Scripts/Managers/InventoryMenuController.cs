/* ===================================================
 * スクリプト名 : InventoryMenuController.cs
 * 用途 : ポーズ画面のカーソル（PlayerControls完全対応版）
 * 解決 : エレコム等のPCパッドやキーボード入力の一元管理
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class InventoryRow{
    public InventoryItemSlot[] slots;
}

public class InventoryMenuController : MonoBehaviour{
    [Header("UI参照")]
    public RectTransform cursorRect;

    [Header("左側のインベントリ設定")]
    public InventoryRow[] inventoryRows;

    [Header("右側の装備先アイコン")]
    public Image equipIconSubAction;
    public Image equipIconSpecial;
    public Image equipIconPassiveA;
    public Image equipIconPassiveB;

    [Header("パッシブ選択用カーソル座標")]
    public RectTransform passiveSlotA_Rect;
    public RectTransform passiveSlotB_Rect;

    [Header("コース退出UI用カーソル座標")]
    public RectTransform stageExitRect;
    public RectTransform dialogYesRect;
    public RectTransform dialogNoRect;

    public int currentRowIndex = 0;
    public int currentColIndex = 0;
    private bool isActive = false;

    private bool isSelectingPassive = false;
    private int selectedPassiveIndex = 0;

    private bool isFocusingStageExit = false;
    private bool isExitDialogOpen = false;
    private bool isYesSelected = false;

    // ▼▼▼ 新規追加：Input Actionのクラスと、UI用の入力クールダウン ▼▼▼
    private PlayerControls input;
    private float uiInputCooldown = 0f;

    void Awake(){
        // 生成したC#クラスを実体化
        input = new PlayerControls();
    }

    private void OnEnable(){
        input.Enable(); // 入力受付スタート
        isActive = true;
        isSelectingPassive = false;
        currentRowIndex = 0;
        currentColIndex = 0;

        if (inventoryRows != null){
            foreach (var row in inventoryRows){
                if (row.slots != null){
                    foreach (var slot in row.slots){
                        if (slot != null) slot.UpdateSlotUI();
                    }
                }
            }
        }
        UpdateCursorPosition();
    }

    private void OnDisable(){
        input.Disable(); // エラー防止のため必ず停止する
        isActive = false;
    }

    void Update(){
        if (!isActive || inventoryRows == null || inventoryRows.Length == 0) return;

        // ====================================================
        // ▼ PlayerControls からの入力取得処理 ▼
        // ====================================================
        if (uiInputCooldown > 0f){
            // ▼修正：Time.deltaTime を Time.unscaledDeltaTime に変更
            uiInputCooldown -= Time.unscaledDeltaTime;
        }

        Vector2 moveDir = input.Player.Move.ReadValue<Vector2>();
        bool isUp = false, isDown = false, isLeft = false, isRight = false;

        // 十字キーやスティックの「連続移動」を防ぎつつ、快適に動かす処理
        if (uiInputCooldown <= 0f && moveDir.sqrMagnitude > 0.1f){
            // 斜め入力時の暴発を防ぐため、XとYで入力が強い方を優先する
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)){
                if (moveDir.x > 0.5f) isRight = true;
                else if (moveDir.x < -0.5f) isLeft = true;
            }else{
                if (moveDir.y > 0.5f) isUp = true;
                else if (moveDir.y < -0.5f) isDown = true;
            }

            if (isUp || isDown || isLeft || isRight){
                uiInputCooldown = 0.2f; // 次のカーソル移動までのクールダウン
            }
        }

        // 画像の設定に合わせて、Attack(Z)かJump(Space)を「決定」として扱う
        bool isSubmit = input.Player.Attack.WasPressedThisFrame() || input.Player.Jump.WasPressedThisFrame();
        // 画像の設定に合わせて、Dash(X)かPause(Esc)を「キャンセル」として扱う
        bool isCancel = input.Player.Dash.WasPressedThisFrame() || input.Player.Pause.WasPressedThisFrame();


        // ====================================================
        // ▼ ここから下はメニューの動作（変更なし） ▼
        // ====================================================

        if (isExitDialogOpen){
            if (isLeft || isRight){
                isYesSelected = !isYesSelected;
                cursorRect.position = isYesSelected ? dialogYesRect.position : dialogNoRect.position;
            }

            if (isSubmit){
                if (isYesSelected) PauseManager.Instance.ConfirmExitCourse();
                else CancelExitDialog();
            }
            else if (isCancel) CancelExitDialog();
            return;
        }

        if (isSelectingPassive){
            if (isLeft || isRight){
                selectedPassiveIndex = (selectedPassiveIndex == 0) ? 1 : 0;
                cursorRect.position = (selectedPassiveIndex == 0) ? passiveSlotA_Rect.position : passiveSlotB_Rect.position;
            }

            if (isSubmit) ConfirmEquipPassive();
            else if (isCancel){
                isSelectingPassive = false;
                UpdateCursorPosition();
            }
            return;
        }

        if (isFocusingStageExit){
            if (isUp){
                isFocusingStageExit = false;
                currentRowIndex = inventoryRows.Length - 1;
                UpdateCursorPosition();
            }else if (isDown){
                isFocusingStageExit = false;
                currentRowIndex = 0;
                UpdateCursorPosition();
            }

            if (isSubmit){
                isExitDialogOpen = true;
                isYesSelected = false;
                PauseManager.Instance.OpenExitDialog();
                cursorRect.position = dialogNoRect.position;
            }
            return;
        }

        bool moved = false;
        int currentRowLength = inventoryRows[currentRowIndex].slots.Length;

        if (isRight){
            currentColIndex++;
            if (currentColIndex >= currentRowLength) currentColIndex = 0;
            moved = true;
        }
        else if (isLeft){
            currentColIndex--;
            if (currentColIndex < 0) currentColIndex = currentRowLength - 1;
            moved = true;
        }else if (isDown){
            currentRowIndex++;
            if (currentRowIndex >= inventoryRows.Length){
                isFocusingStageExit = true;
                cursorRect.position = stageExitRect.position;
            }
            else moved = true;
        }else if (isUp){
            currentRowIndex--;
            if (currentRowIndex < 0){
                isFocusingStageExit = true;
                cursorRect.position = stageExitRect.position;
            }
            else moved = true;
        }

        if (moved){
            int newRowLength = inventoryRows[currentRowIndex].slots.Length;
            if (currentColIndex >= newRowLength) currentColIndex = newRowLength - 1;
            UpdateCursorPosition();
        }

        if (isSubmit) EquipSelectedItem();
    }

    private void CancelExitDialog(){
        isExitDialogOpen = false;
        PauseManager.Instance.CloseExitDialog();
        cursorRect.position = stageExitRect.position;
    }

    private void UpdateCursorPosition(){
        if (cursorRect != null && inventoryRows.Length > currentRowIndex && inventoryRows[currentRowIndex].slots.Length > currentColIndex){
            InventoryItemSlot targetSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
            if (targetSlot != null) cursorRect.position = targetSlot.transform.position;
        }
    }

    private void EquipSelectedItem(){
        InventoryItemSlot selectedSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
        ItemInventoryData selectedItem = selectedSlot.itemData;
        if (selectedItem == null) return;

        int level = 0;
        if (GameManager.Instance != null) level = GameManager.Instance.GetItemLevel(selectedItem.itemId);

        if (selectedItem.category == ItemCategory.SubAction && level <= 0) level = 1;
        if (selectedItem.itemId == 21 && level <= 0) level = 1;
        if (level <= 0) return;

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

        if (pc != null){
            if (selectedPassiveIndex == 0 && pc.equipPassiveB == selectedItem){
                pc.equipPassiveB = null;
                if (equipIconPassiveB != null) equipIconPassiveB.color = new Color(1, 1, 1, 0);
            }else if (selectedPassiveIndex == 1 && pc.equipPassiveA == selectedItem){
                pc.equipPassiveA = null;
                if (equipIconPassiveA != null) equipIconPassiveA.color = new Color(1, 1, 1, 0);
            }
        }

        if (selectedPassiveIndex == 0){
            if (equipIconPassiveA != null){
                equipIconPassiveA.sprite = selectedItem.icon;
                equipIconPassiveA.color = new Color(1, 1, 1, 1);
            }
            if (pc != null) pc.equipPassiveA = selectedItem;
            if (GameManager.Instance != null) GameManager.Instance.currentEquipPassiveA = selectedItem;
        }else{
            if (equipIconPassiveB != null){
                equipIconPassiveB.sprite = selectedItem.icon;
                equipIconPassiveB.color = new Color(1, 1, 1, 1);
            }
            if (pc != null) pc.equipPassiveB = selectedItem;
            if (GameManager.Instance != null) GameManager.Instance.currentEquipPassiveB = selectedItem;
        }

        if (pc != null) pc.ApplyPassiveEffects();
        if (PauseManager.Instance != null) PauseManager.Instance.UpdatePersonalData();

        isSelectingPassive = false;
        UpdateCursorPosition();
    }

    private void UpdateInventoryVisibility(){
        if (GameManager.Instance == null) return;
        foreach (var row in inventoryRows){
            foreach (var slot in row.slots){
                if (slot != null && slot.itemData != null){
                    int level = GameManager.Instance.GetItemLevel(slot.itemData.itemId);
                    Image iconImage = slot.GetComponent<Image>();
                    if (iconImage != null) iconImage.color = level > 0 ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0f);
                }
            }
        }
    }
}