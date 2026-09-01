/* ===================================================
 * スクリプト名 : InventoryItemSlot.cs
 * Version : Ver0.02
 * 用途 : 各スロットに「自分が何のアイテムを持っているか」を持たせる
 * 拡張 : GameManagerと連携し、所持レベルに応じた表示（無所持時の透明化含む）を実装
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour{
    [Header("このスロットに入っているアイテム")]
    public ItemInventoryData itemData;
    public Image iconImage;

    [Header("星のアイコンUI（左から順にセット）")]
    [Tooltip("子オブジェクトにある星のGameObjectを3つ登録してください")]
    public GameObject[] starIcons;

    private void Start(){
        UpdateSlotUI();
    }

    // GameManagerのセーブデータと連動させる ▼▼▼
    public void UpdateSlotUI(){
        if (itemData == null) return;

        int currentLevel = 0;
        if (GameManager.Instance != null){
            currentLevel = GameManager.Instance.GetItemLevel(itemData.itemId);
        }

        // ▼▼▼ 新規追加：アクションコマンド（緑枠）は絶対に未所持にしない ▼▼▼
        if (itemData.category == ItemCategory.SubAction && currentLevel <= 0){
            currentLevel = 1;
        }

        // （※おまけ）もし特定のスペシャル技（例: IDが21のもの）も初期から強制所持させたい場合
        if (itemData.itemId == 21 && currentLevel <= 0){
            currentLevel = 1;
        }

        // 2. アイコン本体の表示・非表示
        if (iconImage != null){
            if (currentLevel > 0){
                iconImage.color = Color.white;
            }else{
                iconImage.color = new Color(1, 1, 1, 0);
            }
        }

        // 3. 星の表示・非表示
        if (starIcons != null && starIcons.Length > 0){
            for (int i = 0; i < starIcons.Length; i++){
                if (starIcons[i] != null){
                    starIcons[i].SetActive(i < currentLevel);
                }
            }
        }
    }
}