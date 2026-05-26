/* ===================================================
 * スクリプト名 : SpecialCollectibleHUD.cs
 * Version : Ver0.01
 * Since : 2026/05/26
 * Update : 2026/05/26
 * 用途 : 特別な収集アイテムの取得状況をUIに表示する
 * =================================================== */
using UnityEngine;
using UnityEngine.UI; // UIを操作するために必須

public class SpecialCollectibleHUD : MonoBehaviour{
    [Header("設定")]
    [Tooltip("現在のステージ番号")]
    public int currentStageId = 1;

    [Tooltip("このステージに配置したアイテムの数（3なら、4個目以降のUIは自動で消えます）")]
    public int totalCollectiblesInStage = 3;

    [Tooltip("画面に並べたUI画像（Image）を左から順にセットしてください")]
    public Image[] collectibleIcons;

    [Header("見た目の設定（半透明方式）")]
    public Color uncollectedColor = new Color(1f, 1f, 1f, 0.3f); // 未取得（半透明）
    public Color collectedColor = new Color(1f, 1f, 1f, 1f);     // 取得済み（くっきり）

    /* * ※もし「透明」ではなく「未取得用のグレーの別の画像」に差し替えたい場合は、
     * 以下の変数のコメントアウトを外し、UpdateHUDの中身を書き換えてください。
     * public Sprite uncollectedSprite;
     * public Sprite collectedSprite;
     */

    void Start(){
        // シーン開始時にセーブデータを確認してUIを更新する
        UpdateHUD();
    }

    // ▼ 取得状況を確認してUIの見た目を切り替えるメソッド ▼
    public void UpdateHUD(){
        int collectedCount = 0; // ▼ 現在いくつ集まったかをカウントする用

        for (int i = 0; i < collectibleIcons.Length; i++){
            // ▼【追加】ステージの総数より多いUI枠は、非表示にして判定もスキップする
            if (i >= totalCollectiblesInStage){
                collectibleIcons[i].gameObject.SetActive(false);
                continue; // これ以降の処理はせず、次の i へ進む
            }

            // 使う枠は表示をONにする
            collectibleIcons[i].gameObject.SetActive(true);

            // セーブデータを確認
            string saveKey = $"Stage_{currentStageId}_SpecialItem_{i}";
            if (PlayerPrefs.GetInt(saveKey, 0) == 1){
                collectibleIcons[i].color = collectedColor;
                collectedCount++; // 取得済みならカウントアップ
            }else{
                collectibleIcons[i].color = uncollectedColor;
            }
        }

        // ▼【応用用】もし全部集めきったかどうかの判定（デバッグ用）
        if (totalCollectiblesInStage > 0 && collectedCount == totalCollectiblesInStage){
            Debug.Log($"ステージ {currentStageId} のスペシャルアイテムをコンプリート！");
            // （※後ほど、ここに「1UP」や「コンプリートフラグの保存」を追加できます）
        }
    }
}