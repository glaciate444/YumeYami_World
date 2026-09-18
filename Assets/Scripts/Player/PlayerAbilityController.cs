/* ===================================================
 * スクリプト名 : PlayerAbilityController.cs
 * 用途 : ダッシュ、ヒップドロップ、大砲、パッシブ装備の管理
 * 連携 : PlayerControllerから分離。安全なpublic設計
 * =================================================== */
using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAbilityController : MonoBehaviour {

    [Header("ダッシュ設定")]
    public ItemInventoryData currentSubActionEquip;
    public float dashCooldown = 0.5f;
    public int maxDashCharges = 3;
    [HideInInspector] public int currentDashCharges;
    public float dashRecoveryTime = 2.0f;
    private float dashRecoveryTimer = 0f;
    [HideInInspector] public bool isDashing;
    private bool canDash = true;

    [Header("ダッシュUI連携（アイコン式）")]
    public Sprite dashOnSprite;
    public Sprite dashOffSprite;
    private PlayerDashHud dashHud;
    public TMP_Text dashText;

    [Header("ダッシュ演出")]
    public AudioClip dashSE;
    public GameObject dashSmokePrefab;

    [Header("ヒップドロップ設定")]
    public GameObject hipDropHitbox;
    [HideInInspector] public bool isHipDropping = false;
    [HideInInspector] public bool isSlowFallingActive = false;

    [Header("装備中のパッシブ")]
    public ItemInventoryData equipPassiveA;
    public ItemInventoryData equipPassiveB;

    [HideInInspector] public int passiveAttackBonus = 0;
    [HideInInspector] public int passiveDefenseBonus = 0;
    [HideInInspector] public float passiveInvincibleBonus = 0f;

    [Header("大砲ギミック設定")]
    [HideInInspector] public bool isInsideCannon = false;
    [HideInInspector] public bool isCannonFlying = false;
    [HideInInspector] public Transform cannonWaitPoint;
    [HideInInspector] public Vector2 savedFlyingVelocity;

    private Rigidbody2D rb;
    private PlayerController pc;
    private PlayerAnimationController animCtrl;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PlayerController>();
        animCtrl = GetComponent<PlayerAnimationController>();

        currentDashCharges = maxDashCharges;
        dashHud = new PlayerDashHud(dashOnSprite, dashOffSprite);
        dashHud.Initialize();
        UpdateDashUI();
    }

    void Update() {
        if (pc.isWarping) return;

        // ▼ 大砲の中にいる間の座標固定
        if (isInsideCannon && cannonWaitPoint != null) {
            transform.position = cannonWaitPoint.position;
            return;
        }

        // ▼ ダッシュチャージの自然回復
        if (currentDashCharges < maxDashCharges) {
            dashRecoveryTimer += Time.deltaTime;
            if (dashRecoveryTimer >= dashRecoveryTime) {
                currentDashCharges++;
                dashRecoveryTimer = 0f;
                UpdateDashUI();
            }
        } else {
            dashRecoveryTimer = 0f;
        }
    }

    void FixedUpdate() {
        // 大砲で飛んでいる最中の勢いを記憶
        if (isCannonFlying) {
            savedFlyingVelocity = rb.linearVelocity;
        }
    }

    // ==========================================
    // サブアクション処理
    // ==========================================
    public void ExecuteSubAction() {
        if (currentDashCharges <= 0) {
            Debug.Log("チャージ不足でアクションが発動できない！");
            return;
        }

        switch (currentSubActionEquip.subActionType) {
            case SubActionType.Dash:
                if (canDash) StartCoroutine(DashRoutine());
                break;
            case SubActionType.Guard:
                Debug.Log("ガード発動！");
                break;
            case SubActionType.HipDrop:
                if (!pc.isGrounded && !isHipDropping) {
                    StartCoroutine(HipDropRoutine());
                }
                break;
            case SubActionType.SlowFall:
                if (!isSlowFallingActive) {
                    currentDashCharges--;
                    UpdateDashUI();
                    isSlowFallingActive = true;
                }
                break;
        }
    }

    private IEnumerator DashRoutine() {
        if (currentSubActionEquip == null) yield break;

        canDash = false;
        isDashing = true;
        currentDashCharges--;
        UpdateDashUI();

        if (SoundManager.instance != null && dashSE != null) SoundManager.instance.PlaySE(dashSE);

        if (dashSmokePrefab != null && pc.groundCheck != null) {
            GameObject smoke = Instantiate(dashSmokePrefab, pc.groundCheck.position, Quaternion.identity);
            float facingDir = Mathf.Sign(transform.localScale.x);
            smoke.transform.localScale = new Vector3(facingDir, 1f, 1f);
        }

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float facingDirection = Mathf.Sign(transform.localScale.x);
        rb.linearVelocity = new Vector2(facingDirection * currentSubActionEquip.actionSpeed, 0f);

        yield return new WaitForSeconds(currentSubActionEquip.actionDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void UpdateDashUI() {
        dashHud?.UpdateChargeIcons(currentDashCharges);
    }

    private IEnumerator HipDropRoutine() {
        isHipDropping = true;
        currentDashCharges--;
        UpdateDashUI();

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        animCtrl.TriggerHipDrop();
        yield return new WaitForSeconds(0.2f);

        if (hipDropHitbox != null) hipDropHitbox.SetActive(true);

        float dropSpeed = currentSubActionEquip.actionSpeed > 0 ? currentSubActionEquip.actionSpeed : 20f;
        float safetyTimer = 0f;

        while (!pc.isGrounded) {
            safetyTimer += Time.deltaTime;
            if (safetyTimer > 3.0f) break;

            rb.linearVelocity = new Vector2(0f, -dropSpeed);

            if (pc.isKnockback) {
                if (hipDropHitbox != null) hipDropHitbox.SetActive(false);
                rb.gravityScale = originalGravity;
                isHipDropping = false;
                yield break;
            }
            yield return null;
        }

        if (hipDropHitbox != null) hipDropHitbox.SetActive(false);
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.2f);
        isHipDropping = false;
    }

    // ==========================================
    // 大砲ギミック処理
    // ==========================================
    public void EnterCannon(Transform waitPoint) {
        isInsideCannon = true;
        cannonWaitPoint = waitPoint;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.simulated = false;
    }

    public void FireFromCannon(Vector2 force) {
        isInsideCannon = false;
        isCannonFlying = true;
        cannonWaitPoint = null;

        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = force;

        if (Mathf.Abs(force.x) > 0.1f) {
            float facingDir = Mathf.Sign(force.x);
            transform.localScale = new Vector3(facingDir, transform.localScale.y, transform.localScale.z);
        }
    }

    public void HandleCannonCollision(Collision2D other) {
        if (isCannonFlying) {
            BreakableBlock block = other.gameObject.GetComponent<BreakableBlock>();
            if (block != null) {
                block.TakeDamage(9999, savedFlyingVelocity.normalized);
                rb.linearVelocity = savedFlyingVelocity;
            }
        }
    }

    // ==========================================
    // パッシブ効果の適用
    // ==========================================
    public void ApplyPassiveEffects() {
        pc.pm.moveSpeed = pc.pm.baseMoveSpeed;
        pc.pm.jumpForce = pc.pm.baseJumpForce;
        passiveAttackBonus = 0;
        passiveDefenseBonus = 0;
        passiveInvincibleBonus = 0f;

        ApplySinglePassive(equipPassiveA);
        ApplySinglePassive(equipPassiveB);
    }

    private void ApplySinglePassive(ItemInventoryData passiveObj) {
        if (passiveObj == null || passiveObj.category != ItemCategory.Passive) return;

        int stars = passiveObj.starLevel;
        switch (passiveObj.passiveType) {
            case PassiveEffectType.Emerald_JumpUp:
                pc.pm.jumpForce += (stars * 1.5f);
                break;
            case PassiveEffectType.Amethyst_SpeedUp:
                pc.pm.moveSpeed += (stars * 1.0f);
                break;
            case PassiveEffectType.Ruby_AttackUp:
                passiveAttackBonus += stars;
                break;
            case PassiveEffectType.Sapphire_DefenseUp:
                passiveDefenseBonus += stars;
                passiveInvincibleBonus += (stars * 0.5f);
                break;
        }
    }
}