/* ===================================================
 * スクリプト名 : MapNode.cs
 * Version : Ver0.02
 * 用途 : マップ上の各ステージの位置を示すポイント。
 * 拡張 : フラグ式進行度への対応とテストモードの完備
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour {
    [Header("ステージ設定")]
    public LevelData myLevelData;

    [Header("UI設定")]
    public Image nodeImage;
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;
    public Color clearedColor = Color.yellow;

    [Header("隣接ノード")]
    public MapNode upNode;
    public MapNode downNode;
    public MapNode leftNode;
    public MapNode rightNode;

    public bool IsUnlocked { get; private set; }
    public bool IsCleared { get; private set; }

    public void SetupNode(){
        if (myLevelData == null) return;

        // ▼▼▼ 修正：テストモード（GameManager不在時）の処理 ▼▼▼
        if (GameManager.Instance == null){
            Debug.LogWarning($"【テストモード】 GameManagerがいないため、{myLevelData.levelName} を強制解放します！");
            IsUnlocked = true;
            IsCleared = true; // 色もクリア済みにして、どこへでも移動可能にする
            UpdateVisuals();
            return; // ここで処理を終える
        }

        // ▲▲▲ 修正ここまで ▲▲▲
        // ▼▼▼ 修正：本番モード（フラグによる判定） ▼▼▼
        // 1. このノード自身がクリア済みかどうかをリストから判定
        IsCleared = GameManager.Instance.IsStageCleared(myLevelData.stageNumber);

        // 2. このノードが解放されているかどうか
        // ※requiredUnlockLevel が 1 以下なら最初から遊べるステージとする
        if (myLevelData.requiredUnlockLevel <= 1){
            IsUnlocked = true;
        }else{
            // requiredUnlockLevel を「解放に必要なクリア済みステージ番号」として扱う
            // 例：requiredUnlockLevel が 3 なら、「ステージ3をクリア済み」なら解放される
            IsUnlocked = GameManager.Instance.IsStageCleared(myLevelData.requiredUnlockLevel);
        }

        // ▲▲▲ 修正ここまで ▲▲▲

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