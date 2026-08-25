/* ===================================================
 * スクリプト名 : MapNode.cs
 * Version : Ver0.04
 * 用途 : マップ上の各ステージの位置を示すポイント。
 * 拡張 : メダル最大枚数の可変対応（LevelData連動）
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour {
    [Header("ステージ設定")]
    public LevelData myLevelData;

    [Header("特殊ノード設定（ショップ用）")]
    [Tooltip("チェックを入れると、LevelData不要のショップマスになります")]
    public bool isShopNode = false;
    public string shopSceneName = "ShopScene";

    [Header("UI設定（ベース枠）")]
    public Image nodeImage;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public Sprite clearedSprite;

    [Header("UI設定（数字アイコン）")]
    public Image numberImage;
    public Sprite lockedNumSprite;
    public Sprite unlockedNumSprite;
    public Sprite clearedNumSprite;

    [Header("UI設定（クラウン）")]
    public Image crownImage;
    public Sprite silverCrownSprite;
    public Sprite goldCrownSprite;

    [Header("隣接ノード")]
    public MapNode upNode;
    public MapNode downNode;
    public MapNode leftNode;
    public MapNode rightNode;

    public bool IsUnlocked { get; private set; }
    public bool IsCleared { get; private set; }

    public void SetupNode(){
        // ショップマスの場合は強制的に解放状態にする
        if (isShopNode){
            IsUnlocked = true;
            IsCleared = false; // ショップは黄色(クリア済)にはならない
            UpdateVisuals();
            if (crownImage != null) crownImage.gameObject.SetActive(false);
            return;
        }

        if (myLevelData == null) return;

        if (GameManager.Instance == null){
            Debug.LogWarning($"【テストモード】 GameManagerがいないため、{myLevelData.levelName} を強制解放します！");
            IsUnlocked = true;
            IsCleared = true;
            UpdateVisuals();
            UpdateCrown();
            return;
        }

        IsCleared = GameManager.Instance.IsStageCleared(myLevelData.stageNumber);
        IsUnlocked = true;

        foreach (int reqStageNum in myLevelData.requiredClearedStageNumbers){
            if (!GameManager.Instance.IsStageCleared(reqStageNum)){
                IsUnlocked = false;
                break;
            }
        }

        if (IsUnlocked){
            foreach (string reqFlag in myLevelData.requiredEventFlags){
                if (!GameManager.Instance.HasEventFlag(reqFlag)){
                    IsUnlocked = false;
                    break;
                }
            }
        }

        UpdateVisuals();
        UpdateCrown();
    }

    private void UpdateVisuals(){
        if (nodeImage != null) nodeImage.color = Color.white;
        if (numberImage != null) numberImage.color = Color.white;

        if (IsCleared){
            if (nodeImage != null && clearedSprite != null) nodeImage.sprite = clearedSprite;
            if (numberImage != null && clearedNumSprite != null) numberImage.sprite = clearedNumSprite;
        }else if (IsUnlocked){
            if (nodeImage != null && unlockedSprite != null) nodeImage.sprite = unlockedSprite;
            if (numberImage != null && unlockedNumSprite != null) numberImage.sprite = unlockedNumSprite;
        }else{
            if (nodeImage != null && lockedSprite != null) nodeImage.sprite = lockedSprite;
            if (numberImage != null && lockedNumSprite != null) numberImage.sprite = lockedNumSprite;
        }
    }

    private void UpdateCrown(){
        if (crownImage == null || myLevelData == null) return;

        // ▼▼▼ 修正：LevelDataから最大枚数を取得する ▼▼▼
        int maxMedals = myLevelData.maxMedals;
        int collectedMedals = 0;

        if (maxMedals <= 0){
            crownImage.gameObject.SetActive(false);
            return;
        }

        // ▼ GameManagerから現在のファイル番号を取得
        int slot = 1;
        if (GameManager.Instance != null) slot = GameManager.Instance.currentSaveSlot;

        for (int i = 0; i < maxMedals; i++){
            // ▼ 末尾に _{slot} を追加して読み込む
            string saveKey = $"Stage_{myLevelData.stageNumber}_SpecialItem_{i}_{slot}";
            if (PlayerPrefs.GetInt(saveKey, 0) == 1){
                collectedMedals++;
            }
        }

        if (collectedMedals == 0){
            // 1枚も取っていない場合は非表示
            crownImage.gameObject.SetActive(false);
        }else if (collectedMedals < maxMedals){
            // 1枚以上、最大枚数未満の場合は銀冠
            crownImage.gameObject.SetActive(true);
            crownImage.sprite = silverCrownSprite;
        }else{
            // 最大枚数（コンプリート）の場合は金冠
            crownImage.gameObject.SetActive(true);
            crownImage.sprite = goldCrownSprite;
        }
        // ▲▲▲ 修正ここまで ▲▲▲
    }
}