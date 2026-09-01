/* ===================================================
 * スクリプト名 : PlayerStomp.cs
 * Version : Ver0.07
 * 用途 : プレイヤーが敵を踏みつけた処理
 * 修正 : 踏みつけ不可（トゲ付きなど）の敵を判定して弾く処理を追加
 * =================================================== */
using UnityEngine;
using System.Collections;

public class PlayerStomp : MonoBehaviour{
    [Header("踏みつけ設定")]
    public int stompDamage = 2;
    public float bounceForce = 12f;

    [Header("効果音")]
    public AudioClip stompSE;

    private Rigidbody2D playerRb;

    void Start(){
        playerRb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other){
        PlayerController pc = GetComponentInParent<PlayerController>();
        if (pc != null && pc.isHipDropping) return;

        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null && !other.CompareTag("Player")){

            if (other.GetComponent<BreakableBlock>() != null){
                return;
            }

            // 踏みつけ不可の敵なら、絶対に踏めない（跳ねない）ようにする ▼▼▼
            TouchDamage enemyTouchDamage = other.GetComponent<TouchDamage>();
            if (enemyTouchDamage != null && !enemyTouchDamage.canBeStomped){
                // Can Be Stomped のチェックが外れている敵なら、ここで処理を中断！
                // （踏めずにそのまま落下し、TouchDamage側でダメージを受けます）
                return;
            }

            float myBottomY = GetComponent<Collider2D>().bounds.min.y;
            float enemyCenterY = other.bounds.center.y;

            if (myBottomY > enemyCenterY - 0.2f){

                int finalStompDamage = stompDamage;
                if (pc != null){
                    finalStompDamage += pc.passiveAttackBonus;
                }

                target.TakeDamage(finalStompDamage, Vector2.down);

                if (SoundManager.instance != null){
                    SoundManager.instance.PlaySE(stompSE);
                }

                if (enemyTouchDamage != null){
                    StartCoroutine(DisableTouchDamageRoutine(enemyTouchDamage));
                }

                if (playerRb != null){
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
                }
            }
        }
    }

    private IEnumerator DisableTouchDamageRoutine(TouchDamage td)
    {
        td.enabled = false;
        yield return new WaitForSeconds(0.5f);
        if (td != null)
        {
            td.enabled = true;
        }
    }
}