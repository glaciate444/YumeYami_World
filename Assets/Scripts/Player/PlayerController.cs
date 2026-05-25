/* ===================================================
 * スクリプト名 : PlayerController.cs
 * Version : Ver0.07
 * Since : 2026/04/01
 * Update : 2026/05/25
 * 用途 : プレイヤー制御
 * 更新 : 効果音追加
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // コルーチンを使うために追加
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour{
    [Header("移動・ジャンプ設定")]
    public float moveSpeed = 8f;
    public float jumpForce = 9f; // 調整済みの値
    [Range(0f, 1f)] public float jumpCutMultiplier = 0.5f;
    public float coyoteTime = 0.15f;    // 空中ジャンプを許容する時間（0.15秒が王道です）
    private float coyoteTimeCounter;    // 現在のタイマーの残り時間

    [Header("接地判定設定")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("ダッシュ設定")]
    public float dashSpeed = 15f;      // ダッシュ中の速度
    public float dashDuration = 0.2f;  // ダッシュしている時間
    public float dashCooldown = 0.5f;  // 次のダッシュができるまでの時間
    public int maxDashCharges = 3;     // ダッシュの最大ストック数
    public int currentDashCharges;     // 現在のストック数
    public float dashRecoveryTime = 2.0f; // 1メモリ回復するまでの秒数
    private float dashRecoveryTimer = 0f;
    private bool isDashing;
    private bool canDash = true;
    // ▼ダッシュ設定のUI連携部分を書き換え
    [Header("ダッシュUI連携（アイコン式）")]
    public Sprite dashOnSprite;  // 黄色いアイコン
    public Sprite dashOffSprite; // 白い（空の）アイコン
    // スクリプト内で見つけたアイコンを格納する配列
    private Image[] dashIcons;

    [Header("ダッシュUI連携")]
    public TMP_Text dashText; // ※アイコンにする場合は後でImageの配列等に変更可能です

    [Header("攻撃設定")]
    public GameObject attackHitbox;    // 攻撃判定用の小オブジェクト
    public float attackDuration = 0.1f; // 攻撃判定が出ている時間
    public float attackCooldown = 0.3f; // 次の攻撃ができるまでの時間
    private bool isAttacking;
    private bool canAttack = true;

    [Header("坂道対策の摩擦マテリアル")]
    public PhysicsMaterial2D zeroFriction; // 動く時・空中の時用
    public PhysicsMaterial2D highFriction; // 立ち止まった時用

    [Header("壁キック設定")]
    public Transform wallCheck;         // 壁判定用の円の中心
    public float wallCheckRadius = 0.2f;// 壁判定の広さ
    public LayerMask wallLayer;         // 「壁」として扱うレイヤー
    public float wallSlidingSpeed = 2f; // 壁をずり落ちる速度
    public Vector2 wallJumpForce = new Vector2(10f, 12f); // Xが横に飛ぶ力、Yが上に飛ぶ力
    public float wallJumpDuration = 0.5f; // 【重要】壁キック直後の「操作無効」時間

    [Header("梯子設定")]
    public float climbSpeed = 5f; // 登る速度
    private bool isNearLadder;    // 梯子に触れているか
    private bool isClimbing;      // 今実際に登っているか
    private float defaultGravity; // 元の重力を記憶しておく用

    private bool isWallTouch;
    private bool isWallSliding;
    private bool isWallJumping;
    private float wallJumpTimer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private PlayerControls inputActions;
    private Animator anim;

    [Header("効果音")]
    public AudioClip jumpSE; // ← ここに追加

    // PlayerController.cs に追加・修正
    [HideInInspector]
    public bool isKnockback; // 外から操作できるように public または [HideInInspector]

    // 動く床から受け取る速度
    [HideInInspector]
    public Vector2 platformVelocity;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale; // 初期重力を記憶
        inputActions = new PlayerControls();

        // ▼【追加】ダッシュチャージの初期化とUI検索
        currentDashCharges = maxDashCharges;

        // ▼【変更】親オブジェクトを探し、子供のImageを全て取得する
        GameObject dashIconContainer = GameObject.FindWithTag("DashText");
        if (dashIconContainer != null){
            // 親オブジェクトの下にあるすべての Image コンポーネントを取得
            dashIcons = dashIconContainer.GetComponentsInChildren<Image>();
        }else{
            Debug.LogWarning("DashTextタグの付いたアイコンの親が見つかりません。");
        }

        UpdateDashUI(); // 初期表示の更新

        // 移動
        inputActions.Player.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += context => moveInput = Vector2.zero;

        // ジャンプ
        inputActions.Player.Jump.performed += context => Jump();
        inputActions.Player.Jump.canceled += context => OnJumpCanceled();

        // --- 追加：ダッシュと攻撃 ---
        // ▼【変更】ダッシュ実行の条件に「チャージが残っているか」を追加
        inputActions.Player.Dash.performed += context => {
            if (canDash && currentDashCharges > 0) StartCoroutine(DashRoutine());
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
        // ▼ 壁に触れているか判定 ▼
        isWallTouch = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        // ▼ 壁ずり落ち判定 ▼
        // 「空中にいる」かつ「壁に触れている」かつ「壁に向かってキーを押している」時にずり落ちる
        if (isWallTouch && !isGrounded && moveInput.x != 0){
            isWallSliding = true;
        }else{
            isWallSliding = false;
        }
        // ▼【追加】壁キック後の操作無効タイマーを減らす ▼
        if (isWallJumping){
            wallJumpTimer -= Time.deltaTime;
            if (wallJumpTimer <= 0){
                isWallJumping = false; // タイマーがゼロになったら操作可能に戻す
            }
        }

        // 向きの反転処理（スケールを使用）
        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);

        // ▼ アニメーションの更新 ▼
        // 1. 歩行判定（左右の入力が少しでもあれば true）
        anim.SetBool("isWalking", Mathf.Abs(moveInput.x) > 0.1f);

        // 2. 接地判定
        anim.SetBool("isGrounded", isGrounded);

        // 【追加】壁ずり落ち中かどうかをAnimatorに教える
        anim.SetBool("isWallSliding", isWallSliding);

        // 現在のYの速度を取得
        float currentVelY = rb.linearVelocity.y;

        // ▼ 梯子に触れている時に「上下」を入力したら登り状態に移行 ▼
        if (isNearLadder && Mathf.Abs(moveInput.y) > 0.1f){
            isClimbing = true;
            anim.SetFloat("velocityY", isClimbing ? 0f : currentVelY);
        }

        // ▼ アニメーションの更新部分に以下を追加 ▼
        anim.SetBool("isClimbing", isClimbing);
        // 上下に入力がある（動いている）時だけ true にする
        // 0.1f だと敏感すぎる場合があるので、0.3f くらいまで上げると安定します
        bool isMovingOnLadder = isClimbing && Mathf.Abs(moveInput.y) > 0.3f;
        anim.SetBool("isClimbingMoving", isMovingOnLadder);

        // ▼ isGrounded の処理を1つにまとめる ▼
        if (isGrounded){
            currentVelY = 0f; // Y方向の揺れを無視
            coyoteTimeCounter = coyoteTime; // 【追加】タイマーを最大値に保つ
        }else{
            if (Mathf.Abs(currentVelY) < 0.05f) currentVelY = 0f; // 極小ノイズ対策
            coyoteTimeCounter -= Time.deltaTime; // 【追加】空中にいる間はタイマーを減らす
        }

        // フィルターを通した綺麗な数値をAnimatorに渡す
        anim.SetFloat("velocityY", currentVelY);

        // ▼ 【追加】今、攻撃ルーチンの真っ最中かどうかをAnimatorに教える ▼
        anim.SetBool("isAttacking", isAttacking);

        // ▼ 【追加】今、ノックバック中かどうかをAnimatorに教える ▼
        anim.SetBool("isKnockback", isKnockback);

        // ▼【追加】ダッシュチャージの自然回復処理 ▼
        if (currentDashCharges < maxDashCharges){
            dashRecoveryTimer += Time.deltaTime;
            if (dashRecoveryTimer >= dashRecoveryTime){
                currentDashCharges++;
                dashRecoveryTimer = 0f; // タイマーリセット
                UpdateDashUI();
            }
        }else{
            dashRecoveryTimer = 0f; // 満タンの時はタイマーを回さない
        }
    }

    void FixedUpdate(){
        // ダッシュ中は通常の移動処理を行わない（ダッシュの速度で上書きされているため）
        if (isDashing) return;
        // ノックバック中は、InputSystemによる移動入力を無視する
        if (isKnockback) return;

        // 壁キックで飛んでいる最中は、通常の左右移動を無視する！ ▼
        if (isWallJumping) return;

        // ▼ 【追加】梯子を登っている最中の専用処理 ▼
        if (isClimbing){
            rb.gravityScale = 0f; // 重力をゼロにして落下を防ぐ

            // X軸（左右）の移動も許可するか、梯子中は上下移動のみにするかで変わります。
            // 今回は少しだけ左右にも動ける王道アクションスタイルにします
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed * 0.5f, moveInput.y * climbSpeed);
            return; // 梯子中はこれ以下の通常の移動処理を全てキャンセルする！
        }else{
            rb.gravityScale = defaultGravity; // 梯子から降りたら重力を元に戻す
        }

        // ▼【変更】壁ずり落ち中の落下速度制限 ▼
        if (isWallSliding){
            // Y軸の落下速度を -wallSlidingSpeed (-2fなど) でストップさせ、ゆっくり落ちるようにする
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
        }else{
            // 1. 通常の移動の処理
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }

        // 1. 移動の処理
        // 【ver0.04変更後】自分の移動速度に、床の速度（platformVelocity.x）を足し合わせる！
        rb.linearVelocity = new Vector2((moveInput.x * moveSpeed) + platformVelocity.x, rb.linearVelocity.y);

        // 【超重要】足し終わったらゼロに戻す（床から降りた瞬間にピタッと止まるようにするため）
        platformVelocity = Vector2.zero;

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
        // 【変更】isGrounded ではなく coyoteTimeCounter が 0 より大きいかで判定する
        if (coyoteTimeCounter > 0f && !isDashing){
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // ▼ 音を鳴らす（安全装置付き）
            if (SoundManager.instance != null){
                SoundManager.instance.PlaySE(jumpSE);
            }

            // 【超重要】ジャンプしたらタイマーを即座にゼロにする（空中での連続ジャンプ防止）
            coyoteTimeCounter = 0f;
        }
        // 2. ▼【追加】壁キック ▼
        else if (isWallSliding){
            isWallJumping = true;                 // 壁キック状態にする
            wallJumpTimer = wallJumpDuration;     // 操作無効タイマーをセット

            // 今プレイヤーが向いている方向（スケールのX）を取得し、その「逆方向」へ飛ぶ
            float facingDir = Mathf.Sign(transform.localScale.x);
            float jumpDirection = -facingDir;

            // 斜め上に向かって力を加える
            rb.linearVelocity = new Vector2(wallJumpForce.x * jumpDirection, wallJumpForce.y);

            // ▼ 音を鳴らす（安全装置付き）
            if (SoundManager.instance != null){
                SoundManager.instance.PlaySE(jumpSE);
            }

            // 飛ぶと同時に、プレイヤーの向き（絵）も反転させる
            transform.localScale = new Vector3(jumpDirection, 1, 1);
        }

        if (isClimbing){
            isClimbing = false;
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

        // ▼【追加】チャージを1消費してUIを更新
        currentDashCharges--;
        UpdateDashUI();

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float facingDirection = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ▼【変更】UI更新用のメソッド（アイコン切り替え版）
    private void UpdateDashUI(){
        // アイコンが見つかっていなければ何もしない
        if (dashIcons == null || dashIcons.Length == 0) return;

        // アイコンの数だけループ処理
        for (int i = 0; i < dashIcons.Length; i++){
            // i番目のアイコンが、現在のチャージ数より小さければON画像、それ以外はOFF画像
            if (i < currentDashCharges){
                dashIcons[i].sprite = dashOnSprite;
            }else{
                dashIcons[i].sprite = dashOffSprite;
            }
        }
    }

    // ▼ 今までの AttackRoutine を上書きします
    private IEnumerator AttackRoutine(){
        canAttack = false;
        isAttacking = true;
        anim.SetTrigger("Attack");

        // 万が一、着地などでアニメーションが途切れてイベントが不発だった時のための「絶対解除タイマー（安全装置）」
        // ※攻撃アニメーション全体（0.5秒）より少し長い 0.6秒 後に、強制的に false に戻します
        Invoke("ResetAttackState", 0.6f);

        // 連続で攻撃できるクールダウン（間隔）
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // アニメーションイベントから呼ばれる処理
    public void OnAttackAnimEnd(){
        ResetAttackState();
    }

    // ▼ 新しく追加：isAttacking を安全に解除する共通の処理
    private void ResetAttackState(){
        isAttacking = false;
        CancelInvoke("ResetAttackState"); // 重複して呼ばれるのを防ぐ
    }
    // 梯子の判定（Trigger）に触れた時と離れた時
    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Ladder")){
            isNearLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("Ladder")){
            isNearLadder = false;
            isClimbing = false; // 梯子から離れたら強制的に登り状態を解除
        }
    }
}