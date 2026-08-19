/* ===================================================
 * スクリプト名 : MapSelectManager.cs
 * Version : Ver0.03
 * 用途 : UIのアイコンを管理し、選んだステージのSceneをロードします。
 * 修正 : フラグ式進行度への対応と、トランジションへの統合
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro用

public class MapSelectManager : MonoBehaviour {
    [Header("ステージデータ")]
    public LevelData targetLevelData; // インスペクターから Level_01_Data を入れる

    [Header("UI参照")]
    public TextMeshProUGUI levelNameText;
    public Button startButton;

    void Start(){
        SetupUI();
    }

    private void SetupUI(){
        if (targetLevelData == null) return;

        // 1. テキストにステージ名を反映
        if (levelNameText != null){
            levelNameText.text = targetLevelData.levelName;
        }

        // ▼▼▼ 修正：フラグ式の進行度チェックへ変更 ▼▼▼
        bool isUnlocked = true;

        if (GameManager.Instance != null){
            // 条件A：必須クリアステージのチェック
            foreach (int reqStageNum in targetLevelData.requiredClearedStageNumbers){
                if (!GameManager.Instance.IsStageCleared(reqStageNum)){
                    isUnlocked = false;
                    break;
                }
            }

            // 条件B：必須イベントフラグのチェック
            if (isUnlocked){
                foreach (string reqFlag in targetLevelData.requiredEventFlags){
                    if (!GameManager.Instance.HasEventFlag(reqFlag)){
                        isUnlocked = false;
                        break;
                    }
                }
            }
        }else{
            // テキストモード（GameManager不在時）は無条件で解放
            Debug.LogWarning("【テストモード】 GameManagerがいないため強制解放します。");
            isUnlocked = true;
        }
        // ▲▲▲ 修正ここまで ▲▲▲

        // ボタンの有効化/無効化
        if (startButton != null){
            startButton.interactable = isUnlocked;
        }

        // もしロックされていたら、名前を隠す演出
        if (!isUnlocked && levelNameText != null){
            levelNameText.text = "??? (Locked)";
        }
    }

    // Startボタンが押された時に呼ばれる（インスペクターのOnClickに紐付ける）
    public void OnClickStartLevel(){
        if (targetLevelData != null){
            // ▼▼▼ 修正：古い直接ロードから、新しいトランジション経由のロードに変更 ▼▼▼
            if (SceneTransitionManager.Instance != null){
                // Course No.XX という文字を出してかっこよく遷移する
                SceneTransitionManager.Instance.LoadCourseByNumber(
                    targetLevelData.sceneName,
                    targetLevelData.stageNumber
                );
            }else{
                // テストプレイ用の保険
                SceneManager.LoadScene(targetLevelData.sceneName);
            }
            // ▲▲▲ 修正ここまで ▲▲▲
        }
    }
}