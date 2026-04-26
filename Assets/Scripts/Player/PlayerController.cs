/* ===================================================
 * スクリプト名 : PlayerController.cs
 * Version : Ver0.02
 * Since : 2026/04/01
 * Update : 2026/04/20
 * 用途 : プレイヤー制御
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // コルーチンを使うために追加

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour{
    [Header("移動・ジャンプ設定")]
    public float moveSpeed = 8f;
    public float jumpForce = 9f; // 調整済みの値
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;

    [Header("接地判定設定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("ダッシュ設定")]
    public float dashSpeed = 15f;      // ダッシュ中の速度
    public float dashDuration = 0.2f;  // ダッシュしている時間
    public float dashCooldown = 0.5f;  // 次のダッシュができるまでの時間
    private bool isDashing;
    private bool canDash = true;

    [Header("攻撃設定")]
    public GameObject attackHitbox;    // 攻撃判定用の小オブジェクト
    public float attackDuration = 0.1f; // 攻撃判定が出ている時間
    public float attackCooldown = 0.3f; // 次の攻撃ができるまでの時間
    private bool isAttacking;
    private bool canAttack = true;

    [Header("坂道対策の摩擦マテリアル")]
    public PhysicsMaterial2D zeroFriction; // 動く時・空中の時用
    public PhysicsMaterial2D highFriction; // 立ち止まった時用

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private PlayerControls inputActions;
    private Animator anim;

    // PlayerController.cs に追加・修正
    [HideInInspector]
    public bool isKnockback; // 外から操作できるように public または [HideInInspector]

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inputActions = new PlayerControls();

        // 移動
        inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;

        // ジャンプ
        inputActions.Player.Jump.performed += context => Jump();
        inputActions.Player.Jump.canceled += context => OnJumpCanceled();

        // --- 追加：ダッシュと攻撃 ---
        // 押された瞬間(performed)にコルーチンを開始
        inputActions.Player.Dash.performed += context => {
            if (canDash) StartCoroutine(DashRoutine());
        };

        inputActions.Player.Attack.performed += context => {
            if (canAttack) StartCoroutine(AttackRoutine());
        };
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update(){
        // ダッシュ中は他の行動（向きの反転や接地判定）を一時停止
        if (isDashing) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 向きの反転処理（スケールを使用）
        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);

        // ▼ アニメーションの更新 ▼
        // 1. 歩行判定（左右の入力が少しでもあれば true）
        anim.SetBool("isWalking", Mathf.Abs(moveInput.x) > 0.1f);

        // 2. 接地判定
        anim.SetBool("isGrounded", isGrounded);

        // 現在のYの速度を取得
        float currentVelY = rb.linearVelocity.y;

        // 1. 地面にいる時は、Y方向の揺れを完全に無視して 0 にする（最強のフィルター）
        if (isGrounded){
            currentVelY = 0f;
        }
        // 2. 空中にいる時の、極小ノイズ対策
        else if (Mathf.Abs(currentVelY) < 0.05f){
            currentVelY = 0f;
        }
        // フィルターを通した綺麗な数値をAnimatorに渡す
        anim.SetFloat("velocityY", currentVelY);
    }

    void FixedUpdate(){
        // ダッシュ中は通常の移動処理を行わない（ダッシュの速度で上書きされているため）
        if (isDashing) return;
        // ノックバック中は、InputSystemによる移動入力を無視する
        if (isKnockback) return;

        // 1. 移動の処理
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        // 2. 坂道滑り落ち防止（摩擦の切り替え）
        // 「地面にいる」かつ「左右の移動入力がゼロ（スティックから手を離している）」場合
        if (isGrounded && Mathf.Abs(moveInput.x) < 0.1f){
            // 摩擦MAXのマテリアルをセットして、斜面でもピタッと止める
            rb.sharedMaterial = highFriction;
        }else{
            // 動いている時やジャンプ中は、摩擦ゼロに戻して壁への張り付きなどを防ぐ
            rb.sharedMaterial = zeroFriction;
        }
    }

    private void Jump(){
        // ダッシュ中・攻撃中はジャンプ不可にする場合は条件を追加
        if (isGrounded && !isDashing){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void OnJumpCanceled(){
        if (rb.linearVelocity.y > 0 && !isDashing){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    // ==========================================
    // コルーチン（時間経過処理）
    // ==========================================

    private IEnumerator DashRoutine(){
        canDash = false;
        isDashing = true;

        // ダッシュ中は重力を無視して水平に飛ぶようにする（お好みで外してもOK）
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // 現在向いている方向（スケールのX）へダッシュ速度を適用
        float facingDirection = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);

        // 指定時間（dashDuration）待機
        yield return new WaitForSeconds(dashDuration);

        // ダッシュ終了・元の状態に戻す
        rb.gravityScale = originalGravity;
        isDashing = false;

        // クールダウン待機
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator AttackRoutine(){
        canAttack = false;
        isAttacking = true;
        anim.SetTrigger("Attack");

        // 攻撃判定をONにする
        if (attackHitbox != null) attackHitbox.SetActive(true);

        // 攻撃の持続時間待機
        yield return new WaitForSeconds(attackDuration);

        // 攻撃判定をOFFにする
        if (attackHitbox != null) attackHitbox.SetActive(false);
        isAttacking = false;

        // クールダウン待機
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}