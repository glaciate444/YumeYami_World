/* ===================================================
 * スクリプト名 : PlayerMovement.cs
 * 用途 : プレイヤーの「歩行・ジャンプ・接地判定・摩擦」を担当
 * 連携 : 物理演算(FixedUpdate)のコア部分を分離
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour{
    [Header("移動・ジャンプ設定")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.15f;
    [HideInInspector] public float coyoteTimeCounter;

    [Header("接地判定設定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    [HideInInspector] public bool isGrounded;

    [Header("坂道対策の摩擦マテリアル")]
    public PhysicsMaterial2D zeroFriction;
    public PhysicsMaterial2D highFriction;

    [Header("効果音")]
    public AudioClip jumpSE;

    [HideInInspector] public Vector2 platformVelocity;

    [HideInInspector] public float baseMoveSpeed;
    [HideInInspector] public float baseJumpForce;

    private Rigidbody2D rb;
    private PlayerController pc;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PlayerController>();

        // パッシブ計算用に素の数値を記憶
        baseMoveSpeed = moveSpeed;
        baseJumpForce = jumpForce;
    }

    void Update(){
        if (pc.isWarping || pc.isInsideCannon) return;

        // 接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded){
            coyoteTimeCounter = coyoteTime;
            pc.isSlowFallingActive = false; // 着地したらゆっくり降下を解除
        }else{
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate(){
        // 他の特殊アクション中は通常の移動演算を行わない
        if (pc.isWarping || pc.isDashing || pc.isHipDropping || pc.isKnockback ||
            pc.isWallJumping || pc.isClimbing || pc.isSwimming ||
            pc.isInsideCannon || pc.isCannonFlying) return;

        float currentVelocityY = rb.linearVelocity.y;

        // 壁ずり落ち・ゆっくり降下のY軸制限
        if (pc.isWallSliding){
            currentVelocityY = Mathf.Clamp(currentVelocityY, -pc.wallSlidingSpeed, float.MaxValue);
        }else if (pc.isSlowFallingActive && currentVelocityY < 0){
            float slowFallSpeed = pc.currentSubActionEquip != null && pc.currentSubActionEquip.actionSpeed > 0 ? pc.currentSubActionEquip.actionSpeed : 2f;
            currentVelocityY = Mathf.Clamp(currentVelocityY, -slowFallSpeed, float.MaxValue);
        }

        // X軸の移動 + 動く床の速度
        rb.linearVelocity = new Vector2((pc.MoveInputX * moveSpeed) + platformVelocity.x, currentVelocityY);
        platformVelocity = Vector2.zero; // 足し終わったらリセット

        // 坂道滑り落ち防止（摩擦の切り替え）
        if (isGrounded && Mathf.Abs(pc.MoveInputX) < 0.1f){
            rb.sharedMaterial = highFriction;
        }else{
            rb.sharedMaterial = zeroFriction;
        }
    }

    // ▼ PlayerControllerから呼ばれるアクション処理
    public bool ExecuteJump(){
        if (pc.isSwimming) return false;

        if (coyoteTimeCounter > 0f && !pc.isDashing && !pc.isHipDropping){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (SoundManager.instance != null && jumpSE != null){
                SoundManager.instance.PlaySE(jumpSE);
            }
            coyoteTimeCounter = 0f;
            return true; // 通常ジャンプに成功したよ！と返事をする
        }
        return false; // ジャンプできなかったよ、と返事をする
    }

    public void ExecuteJumpCancel(){
        if (rb.linearVelocity.y > 0 && !pc.isDashing){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }
}