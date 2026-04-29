/* ===================================================
 * スクリプト名 : TouchDamage.cs
 * Version : Ver0.03
 * Since : 2026/04/03
 * Update : 2026/04/29
 * 用途 : 敵接触ダメージ
 * 更新内容 : 当たり判定（Bounds）を利用した完璧な踏みつけ判定
 * =================================================== */
using UnityEngine;

public class TouchDamage : MonoBehaviour{
    [Header("ダメージ設定")]
    public int damage = 1;

    [Header("踏みつけ対策")]
    [Tooltip("チェックを入れると、上から踏まれた時にプレイヤーへダメージを与えません（敵用）")]
    public bool canBeStomped = true;

    // 衝突判定（Triggerの場合はOnTriggerEnter2D）
    private void OnCollisionEnter2D(Collision2D other){
        // 相手が IDamageable を持っているか確認
        IDamageable target = other.gameObject.GetComponent<IDamageable>();

        if (target != null && other.gameObject.CompareTag("Player")){

            // ▼ 踏みつけ時の相打ち防止処理 ▼
            if (canBeStomped){
                Rigidbody2D playerRb = other.gameObject.GetComponent<Rigidbody2D>();

                // 【改善】プレイヤーの「足元（コライダーの一番下のY座標）」を取得する
                float playerBottomY = other.collider.bounds.min.y;
                
                // 敵の中心Y座標
                float enemyCenterY = transform.position.y;

                // 足元が、敵の中心より少しでも上にあれば「踏んでいる」とみなす
                bool isPlayerAbove = playerBottomY > enemyCenterY - 0.2f;

                // プレイヤーが上にいるならノーダメージ
                // （※PlayerStompが先に処理されて上方向に跳ねている可能性も考慮し、Y速度の制限は外しました）
                if (isPlayerAbove){
                    Debug.Log("踏んで攻撃したためノーダメージ");
                    return; // ここで処理を止めて、下の TakeDamage を呼ばないようにする
                }else{
                    Debug.LogWarning("相打ち発生！ PlayerBottom: " + playerBottomY + " / EnemyCenter: " + enemyCenterY);
                }
            }

            // 敵からプレイヤーへの方向を計算
            Vector2 dir = (other.transform.position - transform.position).normalized;
            target.TakeDamage(damage, dir);
        }
    }
}