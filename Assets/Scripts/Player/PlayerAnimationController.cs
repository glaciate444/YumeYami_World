/* ===================================================
 * スクリプト名 : PlayerAnimationController.cs
 * 用途 : プレイヤーのアニメーション管理（分離版）
 * 連携 : PlayerController の状態を監視して自動でアニメを切り替える
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : MonoBehaviour{
    private Animator anim;
    private PlayerController pc;
    private Rigidbody2D rb;

    void Awake(){
        anim = GetComponent<Animator>();
        pc = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update(){
        if (pc == null || anim == null || rb == null) return;

        // ワープ中、または大砲の中にいる間はアニメーションを強制停止（歩行させない）
        if (pc.isWarping || pc.isInsideCannon){
            anim.SetBool("isWalking", false);
            return;
        }

        // 1. 歩行判定（左右の入力が少しでもあれば true）
        anim.SetBool("isWalking", Mathf.Abs(pc.MoveInputX) > 0.1f);

        // 2. 接地判定
        anim.SetBool("isGrounded", pc.isGrounded);

        // 3. 壁ずり落ち判定
        anim.SetBool("isWallSliding", pc.isWallSliding);

        // 4. 水中判定
        anim.SetBool("isSwimming", pc.isSwimming);
        anim.SetBool("isSwimmingMoving", pc.isSwimming && (Mathf.Abs(pc.MoveInputX) > 0.1f || Mathf.Abs(pc.MoveInputY) > 0.1f));

        // 5. 梯子判定
        anim.SetBool("isClimbing", pc.isClimbing);
        anim.SetBool("isClimbingMoving", pc.isClimbing && Mathf.Abs(pc.MoveInputY) > 0.3f);

        // 6. 特殊状態
        anim.SetBool("isCannonFlying", pc.isCannonFlying);
        anim.SetBool("isAttacking", pc.isAttacking);
        anim.SetBool("isKnockback", pc.isKnockback);

        // 7. Y軸の速度（落下やジャンプ用）
        float currentVelY = rb.linearVelocity.y;

        // 地面にいる時、ヒップドロップ中、大砲飛行中は落下アニメにさせないため数値を0に偽装する
        if (pc.isGrounded || pc.isHipDropping || pc.isCannonFlying){
            currentVelY = 0f;
        }else if (Mathf.Abs(currentVelY) < 0.05f){
            currentVelY = 0f; // 極小ノイズ対策
        }

        anim.SetFloat("velocityY", currentVelY);
    }

    // ==========================================
    // 単発アクション用のトリガー（PlayerControllerから呼ばれる）
    // ==========================================
    public void TriggerAttack(){
        if (anim != null) anim.SetTrigger("Attack");
    }

    public void TriggerHipDrop(){
        if (anim != null) anim.SetTrigger("HipDrop");
    }

    public void PlayGoalAction(){
        if (anim != null){
            anim.SetBool("isWalking", false);
            anim.SetFloat("velocityY", 0f);
            // anim.SetTrigger("Goal"); // ポーズが完成したらコメントアウトを外す
        }
    }
}