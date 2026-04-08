/* ===================================================
 * スクリプト名 : 落下判定用スクリプト
 * Version : Ver0.01
 * Update : 2026/04/08
 * 用途 : 落下した判定
 * =================================================== */

using UnityEngine;

public class FallZone : MonoBehaviour{
    private void OnTriggerEnter2D(Collider2D other){
        // プレイヤーが落ちてきたら
        if (other.CompareTag("Player")){
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null){
                // 即死させる（最大HP分のダメージを与える）
                health.TakeDamage(health.maxHealth, Vector2.zero);
            }
        }
    }
}