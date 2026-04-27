/* ===================================================
 * スクリプト名 : MapSelectManager.cs
 * Version : Ver0.02
 * Since : 2026/04/27
 * Update : 2026/04/28
 * 用途 : UIのアイコンを管理し、選んだステージのSceneをロードします。
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro用

public class MapSelectManager : MonoBehaviour{
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

        // 2. GameManagerの進行度をチェックして、ボタンの有効/無効を切り替える
        if (GameManager.Instance != null){
            // プレイヤーの進行度が、ステージの要求レベル以上なら true (遊べる)
            bool isUnlocked = GameManager.Instance.unlockedStageLevel >= targetLevelData.requiredUnlockLevel;

            startButton.interactable = isUnlocked;

            // もしロックされていたら、名前を隠す演出
            if (!isUnlocked && levelNameText != null){
                levelNameText.text = "??? (Locked)";
            }
        }
    }

    // Startボタンが押された時に呼ばれる（インスペクターのOnClickに紐付ける）
    public void OnClickStartLevel(){
        if (targetLevelData != null){
            // ScriptableObjectに設定されたScene名を読み込んでロードする！
            SceneManager.LoadScene(targetLevelData.sceneName);
        }
    }
}