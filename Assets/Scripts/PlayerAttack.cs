using UnityEngine;

public class PlayerAttack : MonoBehaviour{
    public int attackPower = 1;

    // Is Triggerのコライダーが何かに触れた時
    private void OnTriggerEnter2D(Collider2D other){
        // 触れた相手が IDamageable (ダメージを受けられる性質) を持っているか確認
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null){
            // 自分（プレイヤー）から見て、敵がどっちの方向にいるか計算
            Vector2 knockbackDir = (other.transform.position - transform.parent.position).normalized;

            // 相手にダメージとノックバック方向を渡す
            target.TakeDamage(attackPower, knockbackDir);
        }
    }
}