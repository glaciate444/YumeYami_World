/* ===================================================
 * スクリプト名 : PlayerStomp.cs
 * Version : Ver0.01
 * Since : 2026/04/29
 * Update : 2026/04/29
 * 用途 : プレイヤーが敵を踏みつけた処理
 * =================================================== */
using UnityEngine;

public class PlayerStomp : MonoBehaviour{
    [Header("踏みつけ設定")]
    public int stompDamage = 2; // 固定2ダメージ
    public float bounceForce = 12f; // 踏んだ後の跳ねる力

    private Rigidbody2D playerRb;

    void Start(){
        // 親オブジェクト（Player本体）のRigidbody2Dを取得して、跳ねる力を加える準備をする
        playerRb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other){
        // ▼ 落下している（Yの速度が0以下）時のみ踏みつけ判定を有効にする ▼
        // これにより、ジャンプの上昇中に下から敵にぶつかった時は踏みつけにならず、通常ダメージを受けます
        if (playerRb != null && playerRb.linearVelocity.y <= 0f){
            // 触れた相手が IDamageable を持っているか確認
            IDamageable target = other.GetComponent<IDamageable>();

            // 相手がいて、かつ自分自身（Player）ではない場合
            if (target != null && !other.CompareTag("Player")){
                // 敵に2ダメージを与える（上から踏んだので、ノックバック方向は真下を指定）
                target.TakeDamage(stompDamage, Vector2.down);

                // プレイヤーを上に跳ねさせる（現在のX速度は維持し、Y速度だけ上書き）
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
            }
        }
    }
}