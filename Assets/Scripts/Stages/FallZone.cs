/* ===================================================
 * スクリプト名 : 落下判定用スクリプト
 * Version : Ver0.02
 * Update : 2026/06/12
 * 用途 : 落下した判定（無敵貫通対応）
 * =================================================== */
using UnityEngine;

public class FallZone : MonoBehaviour{
    private void OnTriggerEnter2D(Collider2D other){
        // プレイヤーが落ちてきたら
        if (other.CompareTag("Player")){
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null){
                // ▼【変更】TakeDamageではなく、無敵を無視する即死メソッドを呼ぶ！
                health.InstantDie();
            }
        }
    }
}