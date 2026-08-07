/* ===================================================
 * スクリプト名 : InventoryItemSlot.cs
 * Version : Ver0.01
 * Since : 2026/07/17
 * Update : 2026/07/17
 * 用途 : 各スロットに「自分が何のアイテムを持っているか」を持たせる
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour {
    [Header("このスロットに入っているアイテム")]
    public ItemInventoryData itemData;

    // ▼▼▼ 新規追加：星のUI管理 ▼▼▼
    [Header("星のアイコンUI（左から順にセット）")]
    [Tooltip("子オブジェクトにある星のGameObjectを3つ登録してください")]
    public GameObject[] starIcons;

    private void Start(){
        UpdateSlotUI();
    }

    // アイテムデータに基づいて星の表示を更新する
    public void UpdateSlotUI(){
        if (itemData != null && starIcons != null && starIcons.Length > 0){
            for (int i = 0; i < starIcons.Length; i++){
                // i (0, 1, 2) が starLevel (1, 2, 3) より小さければ表示
                starIcons[i].SetActive(i < itemData.starLevel);
            }
        }
    }
}