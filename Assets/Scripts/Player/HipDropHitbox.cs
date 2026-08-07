/* ===================================================
 * スクリプト名 : HipDropHitbox.cs
 * Version : Ver0.01
 * Since : 2026/07/21
 * Update : 2026/07/21
 * 用途 : ヒップドロップの判定を作る
 * 更新 : 新規作成
 * =================================================== */
using UnityEngine;

public class HipDropHitbox : MonoBehaviour {
    private PlayerController pc;

    void Start(){
        // 親オブジェクト（Player）についているPlayerControllerを取得
        pc = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other){
        // 触れた相手がダメージを受けられるか（敵や壊せるブロックか）
        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null && !other.CompareTag("Player")){
            // 装備中のアイテム（SO）から攻撃力を取得。設定されていなければデフォルトの1
            int damage = 1;

            if (pc != null && pc.currentSubActionEquip != null){
                damage = pc.currentSubActionEquip.attackPower + pc.passiveAttackBonus;
            }

            // 真下に向かってノックバック/衝撃を与える
            target.TakeDamage(damage, Vector2.down);
        }
    }
}