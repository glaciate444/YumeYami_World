/* ===================================================
 * スクリプト名 : TouchDamage.cs
 * Version : Ver0.04
 * 用途 : 敵接触ダメージ
 * 修正 : 無効化時の強制実行バグ（Unity仕様）への対策
 * =================================================== */
using UnityEngine;

public class TouchDamage : MonoBehaviour
{
    [Header("ダメージ設定")]
    public int damage = 1;
    public float impact = 5f;

    [Header("踏みつけ対策")]
    public bool canBeStomped = true;

    private void OnCollisionEnter2D(Collision2D other){
        // スクリプトがOFFの時は絶対に処理をしない（相打ち防止）
        if (!this.enabled) return;

        IDamageable target = other.gameObject.GetComponentInParent<IDamageable>();
        PlayerController pc = other.gameObject.GetComponentInParent<PlayerController>();

        if (target != null && (other.gameObject.CompareTag("Player") || pc != null)){

            if (canBeStomped){
                float playerBottomY = other.collider.bounds.min.y;
                float enemyCenterY = GetComponent<Collider2D>().bounds.center.y;

                bool isFalling = false;
                if (pc != null){
                    isFalling = pc.GetComponent<Rigidbody2D>().linearVelocity.y <= 0.1f;
                }

                bool isPlayerAbove = playerBottomY > enemyCenterY - 0.4f;

                if (isPlayerAbove && isFalling){
                    return;
                }
            }

            Vector2 dir = (other.transform.position - transform.position).normalized;
            target.TakeDamage(damage, dir * impact);
        }
    }
}