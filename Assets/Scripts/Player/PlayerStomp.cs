/* ===================================================
 * スクリプト名 : PlayerStomp.cs
 * Version : Ver0.05
 * 用途 : プレイヤーが敵を踏みつけた処理
 * 更新内容 : 装備によるダメージ補正値
 * =================================================== */
using UnityEngine;

public class PlayerStomp : MonoBehaviour{
    [Header("踏みつけ設定")]
    public int stompDamage = 2; // 固定2ダメージ
    public float bounceForce = 12f; // 踏んだ後の跳ねる力

    [Header("効果音")]
    public AudioClip stompSE;

    private Rigidbody2D playerRb;

    void Start(){
        playerRb = GetComponentInParent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other){
        PlayerController pc = GetComponentInParent<PlayerController>();
        if (pc != null && pc.isHipDropping) return; // ヒップドロップ中は通常の踏みつけを無効化

        IDamageable target = other.GetComponent<IDamageable>();

        if (target != null && !other.CompareTag("Player")){

            if (other.GetComponent<BreakableBlock>() != null) {
                return; 
            }

            // ▼【超重要・修正】速度ではなく「位置（高さ）」で判定する ▼
            // 鳥に乗った瞬間に物理演算でY速度が0になっても、確実に踏めるようにします
            
            // プレイヤーの足元（このセンサー自体）の一番下のY座標
            float myBottomY = GetComponent<Collider2D>().bounds.min.y;
            
            // 敵（相手）の当たり判定のど真ん中のY座標
            float enemyCenterY = other.bounds.center.y;

            // 自分の足元が、敵のど真ん中より上にあれば「踏んだ」とみなす
            if (myBottomY > enemyCenterY - 0.2f){

                // ▼▼▼ 修正：踏みつけダメージにパッシブを加算 ▼▼▼
                int finalStompDamage = stompDamage;
                if (pc != null){
                    finalStompDamage += pc.passiveAttackBonus;
                }

                // 敵にダメージを与える
                target.TakeDamage(finalStompDamage, Vector2.down);

                if (SoundManager.instance != null){
                    SoundManager.instance.PlaySE(stompSE);
                }

                // プレイヤーを上に跳ねさせる
                if (playerRb != null){
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
                }
            }
        }
    }
}