/* ===================================================
 * スクリプト名 : InventoryMenuController.cs
 * Version : Ver0.01
 * Since : 2026/07/15
 * Update : 2026/07/15
 * 用途 : ポーズ画面のカーソル
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;

// ▼ インスペクターで行ごとにスロットを管理するためのクラス（二次元配列の代わり）
[System.Serializable]
public class InventoryRow {
    [Tooltip("この行に含まれるスロット（左から順にセット）")]
    public RectTransform[] slots;
}

public class InventoryMenuController : MonoBehaviour {
    [Header("UI参照")]
    public RectTransform cursorRect;

    [Header("左側のインベントリ設定")]
    [Tooltip("上から順に行を設定します（0:緑枠, 1:青枠, 2:黄枠など）")]
    public InventoryRow[] inventoryRows;

    [Header("現在の状態（デバッグ用）")]
    public int currentRowIndex = 0;
    public int currentColIndex = 0;
    private bool isActive = false;

    private void OnEnable(){
        isActive = true;
        // ポーズを開いた時は常に一番左上にリセット
        currentRowIndex = 0;
        currentColIndex = 0;
        UpdateCursorPosition();
    }

    private void OnDisable(){
        isActive = false;
    }

    void Update(){
        if (!isActive || inventoryRows == null || inventoryRows.Length == 0) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

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
        // エラー防止：行やスロットが未設定の場合は弾く
        if (cursorRect != null &&
            inventoryRows.Length > currentRowIndex &&
            inventoryRows[currentRowIndex].slots.Length > currentColIndex){
            RectTransform targetSlot = inventoryRows[currentRowIndex].slots[currentColIndex];
            if (targetSlot != null){
                cursorRect.position = targetSlot.position;
            }
        }
    }

    private void EquipSelectedItem(){
        // 今後、ここで選択中のスロットの ItemInventoryData を取得し、
        // プレイヤーに反映させつつ、右側の対応する装備枠（XやCなど）のUIアイコンを書き換えます。
        Debug.Log($"行 {currentRowIndex}、列 {currentColIndex} のアイテムを装備として選択しました！");
    }
}