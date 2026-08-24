/* ===================================================
 * スクリプト名 : SpecialCollectibleHUD.cs
 * Version : Ver0.02
 * 用途 : 特別な収集アイテムの取得状況をUIに表示する
 * 修正 : 多重セーブデータ（スロット別保存）への完全対応
 * =================================================== */
using UnityEngine;
using UnityEngine.UI;

public class SpecialCollectibleHUD : MonoBehaviour{
    [Header("設定")]
    [Tooltip("現在のステージ番号")]
    public int currentStageId = 1;

    [Tooltip("このステージに配置したアイテムの数（3なら、4個目以降のUIは自動で消えます）")]
    public int totalCollectiblesInStage = 3;

    [Tooltip("画面に並べたUI画像（Image）を左から順にセットしてください")]
    public Image[] collectibleIcons;

    [Header("見た目の設定（半透明方式）")]
    public Color uncollectedColor = new Color(1f, 1f, 1f, 0.3f);
    public Color collectedColor = new Color(1f, 1f, 1f, 1f);

    void Start(){
        UpdateHUD();
    }

    public void UpdateHUD(){
        int collectedCount = 0;

        // ▼▼▼ 新規追加：GameManagerから現在のファイル番号（スロット）を取得 ▼▼▼
        int slot = 1;
        if (GameManager.Instance != null){
            slot = GameManager.Instance.currentSaveSlot;
        }
        // ▲▲▲ 新規追加ここまで ▲▲▲

        for (int i = 0; i < collectibleIcons.Length; i++){
            if (i >= totalCollectiblesInStage){
                collectibleIcons[i].gameObject.SetActive(false);
                continue;
            }

            collectibleIcons[i].gameObject.SetActive(true);

            // ▼▼▼ 修正：末尾に _{slot} を追加してスロット別のデータを読み込む ▼▼▼
            string saveKey = $"Stage_{currentStageId}_SpecialItem_{i}_{slot}";

            if (PlayerPrefs.GetInt(saveKey, 0) == 1){
                collectibleIcons[i].color = collectedColor;
                collectedCount++;
            }else{
                collectibleIcons[i].color = uncollectedColor;
            }
            // ▲▲▲ 修正ここまで ▲▲▲
        }

        if (totalCollectiblesInStage > 0 && collectedCount == totalCollectiblesInStage){
            Debug.Log($"ステージ {currentStageId} のスペシャルアイテムをコンプリート！");
        }
    }
}