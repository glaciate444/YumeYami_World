using UnityEngine;

public class PlayerAttack : MonoBehaviour{
    [Header("基礎攻撃力（素手）")]
    public int attackPower = 1;

    [Header("現在の装備（赤枠）")]
    public ItemInventoryData currentWeaponEquip;

    // 敵に当てた時の効果音
    [Header("効果音")]
    public AudioClip hitSE;

    private void OnTriggerEnter2D(Collider2D other){
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null){
            // ▼ 最終的な攻撃力を計算（基礎 ＋ 装備）
            int finalAttackPower = attackPower;
            if (currentWeaponEquip != null){
                finalAttackPower += currentWeaponEquip.attackPower;
            }

            // パッシブ(ルビー)の攻撃力を加算
            PlayerController pc = GetComponentInParent<PlayerController>();
            if (pc != null){
                finalAttackPower += pc.passiveAttackBonus;
            }

            Vector2 knockbackDir = (other.transform.position - transform.parent.position).normalized;
            target.TakeDamage(finalAttackPower, knockbackDir);

            // ダメージを与えた瞬間にヒット音を鳴らす
            if (SoundManager.instance != null && hitSE != null){
                SoundManager.instance.PlaySE(hitSE);
            }
        }
    }
}