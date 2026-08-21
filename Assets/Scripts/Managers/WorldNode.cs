/* ===================================================
 * スクリプト名 : WorldNode.cs
 * Version : Ver0.02
 * 用途 : 各レベルの入り口となるアイコン用のスクリプトです。
 * 拡張 : フラグ式進行度への対応とテストモードの完備
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class WorldNode : MonoBehaviour {
    [Header("ワールド設定")]
    public WorldData myWorldData;

    [Header("UI設定")]
    public Image nodeImage;
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;

    [Header("隣接ノード")]
    public WorldNode upNode;
    public WorldNode downNode;
    public WorldNode leftNode;
    public WorldNode rightNode;

    public bool IsUnlocked { get; private set; }

    public void SetupNode(){
        if (myWorldData == null) return;

        // ▼▼▼ テストモード（GameManager不在時）の処理 ▼▼▼
        if (GameManager.Instance == null){
            Debug.LogWarning($"【テストモード】 GameManagerがいないため、ワールド {myWorldData.worldNumber} を強制解放します！");
            IsUnlocked = true;
            UpdateVisuals();
            return;
        }

        // フラグリストによる厳密な判定
        IsUnlocked = true; // 最初は解放状態としておく

        // 条件：必須イベントフラグのチェック
        foreach (string reqFlag in myWorldData.requiredEventFlags){
            // GameManager の eventFlags リストに、要求されたフラグが含まれているか？
            if (!GameManager.Instance.HasEventFlag(reqFlag)){
                IsUnlocked = false; // 1つでもフラグが足りなければロック！
                break;
            }
        }

        UpdateVisuals();
    }

    private void UpdateVisuals(){
        if (nodeImage == null) return;
        nodeImage.color = IsUnlocked ? unlockedColor : lockedColor;
    }
}