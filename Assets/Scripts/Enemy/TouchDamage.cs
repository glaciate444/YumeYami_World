/* ===================================================
 * スクリプト名 : TouchDamage.cs
 * 用途 : 敵接触ダメージ（元の安定版に戻し、Pivotズレのみ修正）
 * =================================================== */
using UnityEngine;

public class TouchDamage : MonoBehaviour{
    [Header("ダメージ設定")]
    public int damage = 1;
    public float impact = 5f;

    [Header("踏みつけ対策")]
    public bool canBeStomped = true;

    private void OnCollisionEnter2D(Collision2D other){
        IDamageable target = other.gameObject.GetComponent<IDamageable>();

        if (target != null && other.gameObject.CompareTag("Player")){

            // ▼ 踏みつけ時の相打ち防止処理 ▼
            if (canBeStomped){
                float playerBottomY = other.collider.bounds.min.y;
                
                // 【根本解決】transform ではなく、コライダー自身の「真ん中」のY座標を取得する！
                // これにより、鳥でもキノコでも、純粋な当たり判定の半分より上なら「踏んだ」と認識されます。
                float enemyCenterY = GetComponent<Collider2D>().bounds.center.y;

                bool isPlayerAbove = playerBottomY > enemyCenterY - 0.2f;

                if (isPlayerAbove){
                    // 上にいるなら、ダメージ処理をキャンセルして PlayerStomp に任せる
                    return; 
                }
            }

            Vector2 dir = (other.transform.position - transform.position).normalized;
            target.TakeDamage(damage, dir * impact);
        }
    }
}