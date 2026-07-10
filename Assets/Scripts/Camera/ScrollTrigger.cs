/* ===================================================
 * スクリプト名 : ScrollTrigger.cs
 * 用途 : 強制スクロールトリガー
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;

public class ScrollTrigger : MonoBehaviour {
    [Header("参照")]
    public AutoScrollManager scrollManager;

    [Header("設定")]
    [Tooltip("trueなら接触時にスクロール開始、falseなら終了")]
    public bool isStartTrigger = true;

    private void OnTriggerEnter2D(Collider2D collision){
        // プレイヤーが接触したら状態を変更
        if (collision.CompareTag("Player")){
            if (scrollManager != null){
                scrollManager.SetScrollState(isStartTrigger);
            }
        }
    }
}