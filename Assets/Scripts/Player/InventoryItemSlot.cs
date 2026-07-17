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
}