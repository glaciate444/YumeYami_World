using UnityEngine;

public class TouchDamage : MonoBehaviour{
    public int damage = 1;

    // 衝突判定（Triggerの場合はOnTriggerEnter2D）
    private void OnCollisionEnter2D(Collision2D collision){
        // 相手が IDamageable を持っているか確認
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();

        if (target != null && collision.gameObject.CompareTag("Player")){
            // 敵からプレイヤーへの方向を計算
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            target.TakeDamage(damage, dir);
        }
    }
}