/* ===================================================
 * スクリプト名 : MapNode.cs
 * Version : Ver0.01
 * Since : 2026/04/28
 * Update : 2026/04/28
 * 用途 : マップ上の各ステージの位置を示すポイント。
 * このノード自身が、どの LevelData を持っているかを知っています。
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour{
    [Header("ステージ設定")]
    public LevelData myLevelData; // このノードが担当するステージデータ

    [Header("UI設定")]
    public Image nodeImage;       // ステージのアイコン（素材ができたら差し替えます）
    public Color lockedColor = Color.gray;   // ロック中の色
    public Color unlockedColor = Color.white;// 解放済みの色
    public Color clearedColor = Color.yellow;// クリア済みの色

    // このノードから移動できる隣のノード（上、下、左、右）
    [Header("隣接ノード")]
    public MapNode upNode;
    public MapNode downNode;
    public MapNode leftNode;
    public MapNode rightNode;

    public bool IsUnlocked { get; private set; }
    public bool IsCleared { get; private set; }

    // ゲーム開始時に呼ばれ、自分の状態をチェックする
    public void SetupNode(){
        if (myLevelData == null) return;

        int currentLevel = 1; // プレイヤーの現在レベル

        // GameManagerがいる場合（本番の挙動）
        if (GameManager.Instance != null)
        {
            currentLevel = GameManager.Instance.unlockedStageLevel;
        }
        // GameManagerがいない場合（Map画面から直接テスト再生した時の挙動）
        else
        {
            Debug.LogWarning($"【テストモード】 GameManagerがいないため、{myLevelData.levelName} を強制解放します！");
            currentLevel = 999; // 全ステージ解放とみなす
        }

        IsUnlocked = currentLevel >= myLevelData.requiredUnlockLevel;
        IsCleared = currentLevel > myLevelData.requiredUnlockLevel;

        UpdateVisuals();
    }

    private void UpdateVisuals(){
        if (nodeImage == null) return;

        if (IsCleared){
            nodeImage.color = clearedColor;
        }else if (IsUnlocked){
            nodeImage.color = unlockedColor;
        }else{
            nodeImage.color = lockedColor;
        }
    }
}