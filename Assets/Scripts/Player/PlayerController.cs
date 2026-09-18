/* ===================================================
 * スクリプト名 : PlayerController.cs
 * Version : Ver0.15
 * Since : 2026/04/01
 * Update : 2026/09/01
 * 用途 : プレイヤー制御
 * 更新 : シーン跨いでアイテム装備を引き継ぐ
 * =================================================== */
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // コルーチンを使うために追加
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour{
    public PlayerMovement pm { get; private set; }
    public bool isGrounded => pm != null && pm.isGrounded;
    public Vector2 platformVelocity { get => pm.platformVelocity; set => pm.platformVelocity = value; }
    public float moveSpeed { get => pm.moveSpeed; set => pm.moveSpeed = value; }
    public float jumpForce { get => pm.jumpForce; set => pm.jumpForce = value; }
    public Transform groundCheck => pm.groundCheck;

    [Header("ダッシュ設定")]
    public ItemInventoryData currentSubActionEquip;
    //public float dashSpeed = 15f;      // ダッシュ中の速度
    //public float dashDuration = 0.2f;  // ダッシュしている時間
    public float dashCooldown = 0.5f;  // 次のダッシュができるまでの時間
    public int maxDashCharges = 3;     // ダッシュの最大ストック数
    public int currentDashCharges;     // 現在のストック数
    public float dashRecoveryTime = 2.0f; // 1メモリ回復するまでの秒数
    private float dashRecoveryTimer = 0f;
    [HideInInspector] public bool isDashing;
    private bool canDash = true;
    // ▼ダッシュ設定のUI連携部分を書き換え
    [Header("ダッシュUI連携（アイコン式）")]
    public Sprite dashOnSprite;  // 黄色いアイコン
    public Sprite dashOffSprite; // 白い（空の）アイコン
    // スクリプト内で見つけたアイコンを格納する配列
    private PlayerDashHud dashHud;

    [Header("ダッシュUI連携")]
    public TMP_Text dashText; // ※アイコンにする場合は後でImageの配列等に変更可能です

    [Header("ダッシュ演出")]
    public AudioClip dashSE;
    public GameObject dashSmokePrefab;

    [Header("ヒップドロップ設定")]
    public GameObject hipDropHitbox; // ヒップドロップ中にONにする判定
    [HideInInspector] public bool isHipDropping = false; // 外から参照できるようにする
    [HideInInspector] public bool isSlowFallingActive = false; //ゆっくり降下

    [Header("攻撃設定")]
    public GameObject attackHitbox;    // 攻撃判定用の小オブジェクト
    public float attackDuration = 0.1f; // 攻撃判定が出ている時間
    public float attackCooldown = 0.3f; // 次の攻撃ができるまでの時間
    [HideInInspector] public bool isAttacking;
    private bool canAttack = true;

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
    [HideInInspector] public bool isClimbing;      // 今実際に登っているか
    private float defaultGravity; // 元の重力を記憶しておく用

    [Header("装備中のパッシブ")]
    public ItemInventoryData equipPassiveA;
    public ItemInventoryData equipPassiveB;

    [HideInInspector] public int passiveAttackBonus = 0;
    [HideInInspector] public int passiveDefenseBonus = 0;
    [HideInInspector] public float passiveInvincibleBonus = 0f;

    [Header("大砲ギミック設定")]
    [HideInInspector] public bool isInsideCannon = false; // 大砲の中にいるか
    [HideInInspector] public bool isCannonFlying = false; // 大砲から発射されて飛んでいる最中か
    // ▼ 追加：大砲の待機ポイントを記憶しておく用
    [HideInInspector] public Transform cannonWaitPoint;
    [HideInInspector] public Vector2 savedFlyingVelocity; // 飛んでいる最中の勢いを記憶する用

    private bool isWallTouch;
    [HideInInspector] public bool isWallSliding;
    [HideInInspector] public bool isWallJumping;
    private float wallJumpTimer;

    [Header("水中設定")]
    public float swimSpeed = 4f;
    [HideInInspector] public bool isSwimming = false;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerControls inputActions;
    private PlayerAnimationController animCtrl;

    [Header("効果音")]
    // 武器を振った時の音
    public AudioClip attackSwingSE;

    // ▼ 外から上キーの入力を読み取るためのプロパティ
    public float MoveInputX => moveInput.x; // ▼ 新規追加
    public float MoveInputY => moveInput.y;
    // ▼ ワープ中に他の動作を止めるためのフラグ
    [HideInInspector] public bool isWarping = false;

    [HideInInspector] public bool isKnockback; // 外から操作できるように public または [HideInInspector]

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        animCtrl = GetComponent<PlayerAnimationController>();
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale; // 初期重力を記憶
        inputActions = new PlayerControls();

        pm = GetComponent<PlayerMovement>();

        // ダッシュチャージの初期化とUI検索
        currentDashCharges = maxDashCharges;

        dashHud = new PlayerDashHud(dashOnSprite, dashOffSprite);
        dashHud.Initialize();

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
            // ポーズ中ではなく、かつ緑枠に何かが装備されている場合のみ実行
            if (Time.timeScale > 0 && currentSubActionEquip != null){
                ExecuteSubAction();
            }
        };

        inputActions.Player.Attack.performed += context => {
            if (canAttack) StartCoroutine(AttackRoutine());
        };

        // ポーズ処理
        inputActions.Player.Pause.performed += context => {
            // シーン内に PauseManager が存在する場合のみポーズを切り替える
            if (PauseManager.Instance != null){
                PauseManager.Instance.TogglePause();
            }
        };
    }
    // シーン開始時にGameManagerから装備を引き継ぐ ▼▼▼
    void Start(){
        if (GameManager.Instance != null){
            // GameManagerが記憶している装備データを自分にセットする
            if (GameManager.Instance.currentEquipSubAction != null){
                currentSubActionEquip = GameManager.Instance.currentEquipSubAction;
            }
            if (GameManager.Instance.currentEquipPassiveA != null){
                equipPassiveA = GameManager.Instance.currentEquipPassiveA;
            }
            if (GameManager.Instance.currentEquipPassiveB != null){
                equipPassiveB = GameManager.Instance.currentEquipPassiveB;
            }

            // 引き継いだパッシブ装備を元に、ステータスを再計算して反映！
            ApplyPassiveEffects();
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update(){
        if (isWarping) return; // ワープ中は一切の操作とアニメーション更新を無効化
        // 大砲の中にいる間は、毎フレーム強制的にWaitPointへ座標を固定する ▼▼▼
        if (isInsideCannon){
            if (cannonWaitPoint != null){
                transform.position = cannonWaitPoint.position;
            }
            return; // ここで return するため、これより下の移動入力処理は一切呼ばれない
        }

        // ダッシュ中は他の行動（向きの反転や接地判定）を一時停止
        if (isDashing) return;

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

        // 現在のYの速度を取得
        float currentVelY = rb.linearVelocity.y;

        // ▼ 梯子に触れている時に「上下」を入力したら登り状態に移行 ▼
        if (isNearLadder && Mathf.Abs(moveInput.y) > 0.1f){
            isClimbing = true;
        }

        // 上下に入力がある（動いている）時だけ true にする
        // 0.1f だと敏感すぎる場合があるので、0.3f くらいまで上げると安定します
        bool isMovingOnLadder = isClimbing && Mathf.Abs(moveInput.y) > 0.3f;

        // ▼ isGrounded の処理を1つにまとめる ▼
        // ▼ 着地時の大砲解除チェックだけここに残す
        if (isGrounded && isCannonFlying && rb.linearVelocity.y <= 0.1f){
            isCannonFlying = false;
        }

        // ヒップドロップ中に加え、大砲で飛んでいる間も数値を0に偽装する ▼▼▼
        if (isHipDropping || isCannonFlying){
            currentVelY = 0f;
        }

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
        if (isWarping) return; // ワープ中は一切の操作とアニメーション更新を無効化
        // ダッシュ中またはヒップドロップ中は通常の移動処理を行わない
        if (isDashing || isHipDropping) return;
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

        // ▼ 【追加】水中を泳いでいる最中の専用処理 ▼
        if (isSwimming){
            // 方向キーの入力がある時は、入力方向へ自由に泳ぐ
            if (moveInput.magnitude > 0.1f){
                rb.linearVelocity = new Vector2(moveInput.x * swimSpeed, moveInput.y * swimSpeed);
            }
            // 入力がない時は、Buoyancy Effector の浮力でプカプカと自然に浮くのを邪魔しない
            return; // 泳ぎ中は、これより下の通常の歩行・落下処理を完全にキャンセル
        }

        // ▼【変更】壁ずり落ち中の落下速度制限 ▼
        if (isInsideCannon) return;

        if (isCannonFlying){
            // ブロックにぶつかって勢いが消される前に、毎フレーム「現在の飛んでいる勢い」を記憶しておく
            savedFlyingVelocity = rb.linearVelocity;
            return;
        }
    }

    private void Jump(){
        if (isSwimming) return;

        // 1. まず通常ジャンプを試みる
        bool didJump = pm.ExecuteJump();

        // 2. もし通常ジャンプしなかった（空中だった）場合のみ、壁キックを試みる
        if (!didJump && isWallSliding){
            isWallJumping = true;
            wallJumpTimer = wallJumpDuration;

            float facingDir = Mathf.Sign(transform.localScale.x);
            float jumpDirection = -facingDir;

            rb.linearVelocity = new Vector2(wallJumpForce.x * jumpDirection, wallJumpForce.y);
            transform.localScale = new Vector3(jumpDirection, 1, 1);
        }

        if (isClimbing){
            isClimbing = false;
        }
    }

    private void OnJumpCanceled(){
        pm.ExecuteJumpCancel();
    }

    // Animationイベントから呼び出すためのメソッド
    public void PlaySwingSE(){
        if (SoundManager.instance != null && attackSwingSE != null){
            SoundManager.instance.PlaySE(attackSwingSE);
        }
    }

    /// <summary>
    /// 緑枠（Xキー）に装備されているアイテムの種類を判定し、対応するアクションを実行する
    /// </summary>
    private void ExecuteSubAction(){
        // 共通のスタミナ（現在のダッシュチャージ）が残っているかチェック
        // ※ガードやヒップドロップでもこのチャージを消費する想定です
        if (currentDashCharges <= 0){
            Debug.Log("チャージ不足でアクションが発動できない！");
            return;
        }

        // 装備アイテムの SubActionType に応じて処理を分ける
        switch (currentSubActionEquip.subActionType){
            case SubActionType.Dash:
                if (canDash) StartCoroutine(DashRoutine());
                break;

            case SubActionType.Guard:
                // TODO: ガード処理の実装（今はテスト用のログだけ）
                Debug.Log("ガード発動！");
                break;

            case SubActionType.HipDrop:
                // ▼ 空中（ジャンプ・落下中）かつ、ヒップドロップ中でない時のみ発動
                if (!isGrounded && !isHipDropping){
                    StartCoroutine(HipDropRoutine());
                }
                break;

            case SubActionType.SlowFall:
                // すでに発動中でなければチャージを消費してフラグをONにする
                if (!isSlowFallingActive){
                    currentDashCharges--;
                    UpdateDashUI();
                    isSlowFallingActive = true;
                }
                break;

            case SubActionType.None:
            default:
                break;
        }
    }

    // ==========================================
    // コルーチン（時間経過処理）
    // ==========================================
    private IEnumerator DashRoutine(){
        // 装備がない場合は発動しない
        if (currentSubActionEquip == null) yield break;

        canDash = false;
        isDashing = true;
        currentDashCharges--;
        UpdateDashUI();

        // 効果音と煙の演出
        if (SoundManager.instance != null && dashSE != null){
            SoundManager.instance.PlaySE(dashSE);
        }

        if (dashSmokePrefab != null && groundCheck != null){
            // 既存の足元判定（groundCheck）の位置に煙を生成
            GameObject smoke = Instantiate(dashSmokePrefab, groundCheck.position, Quaternion.identity);

            // プレイヤーの向きに合わせて、煙の反転（向き）を合わせる
            float facingDir = Mathf.Sign(transform.localScale.x);
            smoke.transform.localScale = new Vector3(facingDir, 1f, 1f);
        }

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float facingDirection = Mathf.Sign(transform.localScale.x);

        // ▼ SOの値を使うように変更
        rb.linearVelocity = new Vector2(facingDirection * currentSubActionEquip.actionSpeed, 0f);

        // ▼ SOの値を使うように変更
        yield return new WaitForSeconds(currentSubActionEquip.actionDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ▼【変更】UI更新用のメソッド（アイコン切り替え版）
    private void UpdateDashUI(){
        dashHud?.UpdateChargeIcons(currentDashCharges);
    }

    // ▼ 今までの AttackRoutine を上書きします
    private IEnumerator AttackRoutine(){
        canAttack = false;
        isAttacking = true;
        animCtrl.TriggerAttack();

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
        if (other.CompareTag("Water")){
            isSwimming = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other){
        // ▼ はしごの処理
        if (other.CompareTag("Ladder")){
            // 子オブジェクト（攻撃判定など）の消失による誤作動を防ぐため、本体が離れたか厳密に確認する
            if (!GetComponent<Collider2D>().IsTouching(other)){
                isNearLadder = false;
                isClimbing = false;
            }
        }

        // ▼ 水面の処理
        if (other.CompareTag("Water")){
            // 攻撃判定ではなく、プレイヤー本体のコライダーが本当に水面から出た時だけ解除する
            if (!GetComponent<Collider2D>().IsTouching(other)){
                isSwimming = false;

                // 水から出る時、上に入力していれば水面ジャンプ
                if (moveInput.y > 0){
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D other){
        // 大砲で飛んでいる最中に何かにぶつかったら
        if (isCannonFlying){
            BreakableBlock block = other.gameObject.GetComponent<BreakableBlock>();
            if (block != null){
                // 1. 特大ダメージ(9999)を与えて問答無用で破壊する！
                // （鉄の箱でも canBreakByHazard がONなら壊せます）
                block.TakeDamage(9999, savedFlyingVelocity.normalized);

                // 2. ぶつかった衝撃でプレイヤーが止まってしまうのを防ぐため、
                // 記憶しておいた「ぶつかる直前の勢い」を再セットして貫通させる！
                rb.linearVelocity = savedFlyingVelocity;
            }
        }
    }

    private IEnumerator HipDropRoutine(){
        isHipDropping = true;
        currentDashCharges--;
        UpdateDashUI();

        // 1. 空中で一瞬止まる（タメ動作）
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        animCtrl.TriggerHipDrop();

        yield return new WaitForSeconds(0.2f); // 0.2秒タメる

        // 2. 急降下開始
        if (hipDropHitbox != null) hipDropHitbox.SetActive(true);

        float dropSpeed = currentSubActionEquip.actionSpeed > 0 ? currentSubActionEquip.actionSpeed : 20f;

        // ▼ 修正：ここにあった rb.linearVelocity = ... を削除し、下のwhileループの中に移動します

        // 3. 地面に着くまで待機
        float safetyTimer = 0f;
        while (!isGrounded){
            safetyTimer += Time.deltaTime;
            if (safetyTimer > 3.0f) break; // 3秒経っても着地しなければ強制終了

            // ▼【超重要】物理衝突で速度が0にされて宙に浮くのを防ぐため、毎フレーム下向きに強制する！
            rb.linearVelocity = new Vector2(0f, -dropSpeed);

            if (isKnockback){
                if (hipDropHitbox != null) hipDropHitbox.SetActive(false);
                rb.gravityScale = originalGravity;
                isHipDropping = false;
                yield break;
            }
            yield return null;
        }

        // 4. 着地時の処理
        if (hipDropHitbox != null) hipDropHitbox.SetActive(false);
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; // 着地時の滑りを防止

        yield return new WaitForSeconds(0.2f);

        isHipDropping = false;
    }
    // ゴール時の演出用メソッド▼
    public void PlayGoalAction(){
        // 1. キーボードやゲームパッドの入力を完全にシャットアウトする
        inputActions.Disable();

        // 2. 移動の速度を強制的にゼロにして、その場でピタッと止める
        rb.linearVelocity = Vector2.zero;

        // 3. アニメーションを「待機」状態に戻す
        animCtrl.PlayGoalAction();
    }

    // 大砲に格納された瞬間の処理
    public void EnterCannon(Transform waitPoint){
        isInsideCannon = true;
        cannonWaitPoint = waitPoint; // 待機ポイントを記憶

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // ▼ 追加：壁や地面に弾き出されるのを防ぐため、物理エンジンの計算から一時的に消す！
        rb.simulated = false;
    }

    // 大砲から発射された瞬間の処理
    public void FireFromCannon(Vector2 force){
        isInsideCannon = false;
        isCannonFlying = true;
        cannonWaitPoint = null;

        // ▼ 追加：物理エンジンに復帰させる
        rb.simulated = true;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = force;

        // ▼ 追加：飛んでいく方向（X軸の力）を見て、自動的に左右を振り向かせる
        if (Mathf.Abs(force.x) > 0.1f){
            float facingDir = Mathf.Sign(force.x); // 右なら1、左なら-1になる
            transform.localScale = new Vector3(facingDir, transform.localScale.y, transform.localScale.z);
        }
    }
    public void ApplyPassiveEffects(){
        // 1. 一旦ステータスを元の基準値（素の状態）に戻す
        pm.moveSpeed = pm.baseMoveSpeed;
        pm.jumpForce = pm.baseJumpForce;
        passiveAttackBonus = 0;
        passiveDefenseBonus = 0;
        passiveInvincibleBonus = 0f;

        // 2. パッシブAとBの効果を乗せる
        ApplySinglePassive(equipPassiveA);
        ApplySinglePassive(equipPassiveB);
    }

    private void ApplySinglePassive(ItemInventoryData passiveObj){
        if (passiveObj == null || passiveObj.category != ItemCategory.Passive) return;

        int stars = passiveObj.starLevel; // 星の数を取得

        switch (passiveObj.passiveType){
            case PassiveEffectType.Emerald_JumpUp:
                pm.jumpForce += (stars * 1.5f);
                break;
            case PassiveEffectType.Amethyst_SpeedUp:
                pm.moveSpeed += (stars * 1.0f);
                break;
            case PassiveEffectType.Ruby_AttackUp:
                passiveAttackBonus += stars; // 星1につき攻撃力+1
                break;
            case PassiveEffectType.Sapphire_DefenseUp:
                passiveDefenseBonus += stars; // 星1につき-1ダメージ
                passiveInvincibleBonus += (stars * 0.5f); // 星1につき無敵時間+0.5秒
                break;
        }
    }
}