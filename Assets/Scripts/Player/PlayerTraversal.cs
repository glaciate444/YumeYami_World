/* ===================================================
 * スクリプト名 : PlayerTraversal.cs
 * 用途 : 壁キック、梯子、水泳など「特殊地形」の処理
 * 連携 : PlayerControllerから分離。安全なpublic設計
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class PlayerTraversal : MonoBehaviour{
    [Header("壁キック設定")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;
    public float wallSlidingSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallJumpDuration = 0.5f;

    [Header("梯子設定")]
    public float climbSpeed = 5f;

    [Header("水中設定")]
    public float swimSpeed = 4f;

    // ▼ すべて他スクリプトから参照可能（エラー防止）にしています
    [HideInInspector] public bool isWallTouch;
    [HideInInspector] public bool isWallSliding;
    [HideInInspector] public bool isWallJumping;
    [HideInInspector] public float wallJumpTimer;

    [HideInInspector] public bool isNearLadder;
    [HideInInspector] public bool isClimbing;
    [HideInInspector] public float defaultGravity;

    [HideInInspector] public bool isSwimming;

    private Rigidbody2D rb;
    private PlayerController pc;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PlayerController>();
        defaultGravity = rb.gravityScale;
    }

    void Update(){
        if (pc.isWarping || pc.isInsideCannon || pc.isDashing) return;

        // ▼ 壁ずり落ち判定
        isWallTouch = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        if (isWallTouch && !pc.isGrounded && pc.MoveInputX != 0){
            isWallSliding = true;
        }else{
            isWallSliding = false;
        }

        // ▼ 壁キック後の操作無効タイマー
        if (isWallJumping){
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0){
                isWallJumping = false;
            }
        }

        // ▼ 梯子へ移行する判定
        if (isNearLadder && Mathf.Abs(pc.MoveInputY) > 0.1f){
            isClimbing = true;
        }
    }

    void FixedUpdate(){
        if (pc.isWarping || pc.isDashing || pc.isHipDropping || pc.isKnockback) return;

        // ▼ 梯子の物理挙動
        if (isClimbing){
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(pc.MoveInputX * pc.moveSpeed * 0.5f, pc.MoveInputY * climbSpeed);
            return;
        }else{
            rb.gravityScale = defaultGravity;
        }

        // ▼ 水中の物理挙動
        if (isSwimming){
            if (new Vector2(pc.MoveInputX, pc.MoveInputY).magnitude > 0.1f){
                rb.linearVelocity = new Vector2(pc.MoveInputX * swimSpeed, pc.MoveInputY * swimSpeed);
            }
            return;
        }
    }

    // ▼ PlayerControllerの Jump() から呼ばれる処理
    public bool ExecuteWallJump(){
        if (isWallSliding){
            isWallJumping = true;
            wallJumpTimer = wallJumpDuration;

            float facingDir = Mathf.Sign(transform.localScale.x);
            float jumpDirection = -facingDir;

            rb.linearVelocity = new Vector2(wallJumpForce.x * jumpDirection, wallJumpForce.y);
            transform.localScale = new Vector3(jumpDirection, 1, 1);
            return true;
        }
        return false;
    }

    // ▼ PlayerControllerの OnTrigger から呼ばれる処理
    public void CheckTriggerEnter(Collider2D other){
        if (other.CompareTag("Ladder")) isNearLadder = true;
        if (other.CompareTag("Water")) isSwimming = true;
    }

    public void CheckTriggerExit(Collider2D other){
        if (other.CompareTag("Ladder") && !GetComponent<Collider2D>().IsTouching(other)){
            isNearLadder = false;
            isClimbing = false;
        }
        if (other.CompareTag("Water") && !GetComponent<Collider2D>().IsTouching(other)){
            isSwimming = false;
            // 水から出る時、上に入力していれば水面ジャンプ
            if (pc.MoveInputY > 0){
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, pc.jumpForce * 0.8f);
            }
        }
    }
}