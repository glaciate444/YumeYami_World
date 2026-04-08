/* ===================================================
 * スクリプト名 : ジャンプ台スクリプト
 * Version : Ver0.01
 * Update : 2026/04/09
 * 用途 : ジャンプ台
 * =================================================== */
using UnityEngine;

public class Spring : MonoBehaviour{
    [Header("跳ね返る力")]
    public float bounceForce = 20f;

    // 上から乗った時（衝突した時）に発動
    private void OnCollisionEnter2D(Collision2D other){
        if (other.gameObject.CompareTag("Player")){
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
            if (rb != null){
                // 現在の落下速度を完全に無視して、上方向へ強制的に速度を上書きする
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);

                // ボヨーンというアニメーションを入れる場合はここ
                Debug.Log("大ジャンプ！");
            }
        }
    }
}