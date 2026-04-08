/* ===================================================
 * スクリプト名 : トラップ用スクリプト
 * Version : Ver0.01
 * Update : 2026/04/08
 * 用途 : トゲだけでなく、将来的に「マグマ」や「回転のこぎり」などにも使い回せる汎用的なスクリプト
 * =================================================== */
using UnityEngine;

public class Hazard : MonoBehaviour{
    public int damageAmount = 3;

    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag("Player")){
            IDamageable player = collision.GetComponent<IDamageable>();
            if (player != null){
                // Tilemap対応：ぶつかった「一番近い表面の位置」を取得
                Vector2 contactPoint = collision.ClosestPoint(transform.position);

                // 接点からプレイヤーの中心に向かって弾き飛ばす
                Vector2 knockbackDir = ((Vector2)collision.transform.position - contactPoint).normalized;

                // 真上や真下に当たった時も少し斜めに飛ぶように補正（お好みで）
                knockbackDir.y = 0.5f;

                player.TakeDamage(damageAmount, knockbackDir.normalized);
            }
        }
    }
}