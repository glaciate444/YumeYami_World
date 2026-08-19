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

        // 【テストモード】 GameManagerがいない場合は無条件で解放
        if (GameManager.Instance == null){
            Debug.LogWarning($"【テストモード】 GameManagerがいないため、{myLevelData.levelName} を強制解放します！");
            IsUnlocked = true;
            IsCleared = true;
            UpdateVisuals();
            return;
        }

        // ▼▼▼ 本番モード（フラグリストによる厳密な判定） ▼▼▼

        // 1. このノード自身がクリア済みかどうか
        IsCleared = GameManager.Instance.IsStageCleared(myLevelData.stageNumber);

        // 2. 解放条件のチェック（最初は true にしておき、条件を満たしていないものがあれば false に落とす）
        IsUnlocked = true;

        // 条件A：必須クリアステージのチェック
        foreach (int reqStageNum in myLevelData.requiredClearedStageNumbers){
            if (!GameManager.Instance.IsStageCleared(reqStageNum)){
                IsUnlocked = false; // 1つでもクリアしてないものがあればロック！
                break;
            }
        }

        // 条件B：必須イベントフラグのチェック（ステージ条件をパスした場合のみ確認）
        if (IsUnlocked){
            foreach (string reqFlag in myLevelData.requiredEventFlags){
                if (!GameManager.Instance.HasEventFlag(reqFlag)){
                    IsUnlocked = false; // 1つでもフラグが足りなければロック！
                    break;
                }
            }
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