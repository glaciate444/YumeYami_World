/* ===================================================
 * スクリプト名 : PlayerController.cs
 * Version : Ver0.20
 * Since : 2026/04/01
 * Update : 2026/09/19
 * 用途 : プレイヤー制御
 * 更新 : リファクタリングにより、大掃除
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
    public PlayerTraversal pt { get; private set; }
    public bool isWallSliding => pt != null && pt.isWallSliding;
    public bool isWallJumping => pt != null && pt.isWallJumping;
    public bool isSwimming => pt != null && pt.isSwimming;
    public bool isClimbing { get => pt != null && pt.isClimbing; set { if (pt != null) pt.isClimbing = value; } }
    public float wallSlidingSpeed => pt != null ? pt.wallSlidingSpeed : 2f;
    // ▼ アビリティ関係の窓口
    public PlayerAbilityController pac { get; private set; }
    public bool isDashing => pac != null && pac.isDashing;
    public bool isHipDropping => pac != null && pac.isHipDropping;
    public bool isSlowFallingActive { get => pac != null && pac.isSlowFallingActive; set { if (pac != null) pac.isSlowFallingActive = value; } }
    public bool isInsideCannon => pac != null && pac.isInsideCannon;
    public bool isCannonFlying { get => pac != null && pac.isCannonFlying; set { if (pac != null) pac.isCannonFlying = value; } }

    // ▼ 装備の引き継ぎ用窓口
    public ItemInventoryData currentSubActionEquip { get => pac != null ? pac.currentSubActionEquip : null; set { if (pac != null) pac.currentSubActionEquip = value; } }
    public ItemInventoryData equipPassiveA { get => pac != null ? pac.equipPassiveA : null; set { if (pac != null) pac.equipPassiveA = value; } }
    public ItemInventoryData equipPassiveB { get => pac != null ? pac.equipPassiveB : null; set { if (pac != null) pac.equipPassiveB = value; } }

    // ▼ パッシブステータス窓口
    public int passiveAttackBonus => pac != null ? pac.passiveAttackBonus : 0;
    public int passiveDefenseBonus => pac != null ? pac.passiveDefenseBonus : 0;
    public float passiveInvincibleBonus => pac != null ? pac.passiveInvincibleBonus : 0f;

    [Header("攻撃設定")]
    public GameObject attackHitbox;    // 攻撃判定用の小オブジェクト
    public float attackDuration = 0.1f; // 攻撃判定が出ている時間
    public float attackCooldown = 0.3f; // 次の攻撃ができるまでの時間
    [HideInInspector] public bool isAttacking;
    private bool canAttack = true;

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
        //defaultGravity = rb.gravityScale; // 初期重力を記憶
        inputActions = new PlayerControls();

        pm = GetComponent<PlayerMovement>();
        pt = GetComponent<PlayerTraversal>();
        pac = GetComponent<PlayerAbilityController>();

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
                pac.ExecuteSubAction();
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
            pac.ApplyPassiveEffects();
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    void Update(){
        if (isWarping) return; // ワープ中は一切の操作とアニメーション更新を無効化

        // ダッシュ中は他の行動（向きの反転や接地判定）を一時停止
        if (isDashing) return;

        // 向きの反転処理（スケールを使用）
        if (moveInput.x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput.x < 0) transform.localScale = new Vector3(-1, 1, 1);

        // 現在のYの速度を取得
        float currentVelY = rb.linearVelocity.y;

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
    }

    private void Jump(){
        if (isSwimming) return;

        bool didJump = pm.ExecuteJump();

        // 通常ジャンプしなかった場合のみ壁キックを試みる
        if (!didJump){
            pt.ExecuteWallJump();
        }

        // ジャンプボタンを押したら梯子から手を離す
        isClimbing = false;
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
        if (pt != null) pt.CheckTriggerEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other){
        if (pt != null) pt.CheckTriggerExit(other);
    }

    private void OnCollisionEnter2D(Collision2D other){
        if (pac != null) pac.HandleCannonCollision(other);
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
}