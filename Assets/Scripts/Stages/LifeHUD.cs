/* ===================================================
 * スクリプト名 : LifeHUD.cs
 * Version : Ver0.01
 * 用途 : 残基と1UPの欠片の数を画面に表示する
 * =================================================== */
using UnityEngine;
using TMPro;

public class LifeHUD : MonoBehaviour {
    [Header("UI連携")]
    public TMP_Text livesText;      // 残基の数字テキスト
    public TMP_Text lifePieceText;  // 欠片の数字テキスト

    void Start() {
        // シーンが始まった時（画面が切り替わった時）に最新の数字にする
        UpdateHUD();
    }

    // ▼ GameManagerから「アイテム取ったよ！」と言われた時に実行される
    public void UpdateHUD() {
        if (GameManager.Instance != null) {
            
            // 残基の更新
            if (livesText != null) {
                // そのまま数字だけ表示（例: "3"）
                livesText.text = GameManager.Instance.currentLives.ToString();
            }

            // 欠片の更新
            if (lifePieceText != null) {
                // "D2" を付けると、5個の時は "05"、99個の時は "99" と2桁で綺麗に揃います
                lifePieceText.text = GameManager.Instance.currentLifePieces.ToString("D2"); 
            }
        }
    }
}