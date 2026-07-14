using UnityEngine;

public class PlayerAttack : MonoBehaviour{
    [Header("基礎攻撃力（素手）")]
    public int attackPower = 1;

    // ▼ 新しくSOの枠を追加
    [Header("現在の装備（赤枠）")]
    public ItemInventoryData currentWeaponEquip;

    private void OnTriggerEnter2D(Collider2D other){
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null){
            // ▼ 最終的な攻撃力を計算（基礎 ＋ 装備）
            int finalAttackPower = attackPower;
            if (currentWeaponEquip != null){
                finalAttackPower += currentWeaponEquip.attackPower;
            }

            Vector2 knockbackDir = (other.transform.position - transform.parent.position).normalized;
            target.TakeDamage(finalAttackPower, knockbackDir);
        }
    }
}