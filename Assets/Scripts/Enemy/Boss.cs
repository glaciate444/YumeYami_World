/* ===================================================
 * スクリプト名 : Boss.cs
 * 用途 : ボスのステータス管理、HPバー連動、登場演出、撃破演出
 * 拡張 : 被弾時の無敵時間（点滅）処理を追加
 * =================================================== */
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
public class Boss : MonoBehaviour, IDamageable {

    public enum BossType { StageBoss, RoomGuarder }

    [Header("ボス基本ステータス")]
    public BossType bossType = BossType.RoomGuarder;
    public string bossName = "大ボス";
    public int maxHp = 50;
    private int currentHp;

    // ▼▼▼ 新規追加：無敵時間の設定 ▼▼▼
    [Header("被弾時の無敵設定")]
    [Tooltip("ダメージを受けた後に無敵になる秒数")]
    public float invincibilityTime = 1.0f;
    [Tooltip("点滅の速さ")]
    public float blinkInterval = 0.1f;
    private bool isInvincible = false;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("UI連携")]
    public Slider bossHpSlider;
    public TMP_Text bossHpText;

    [Header("ルームガーダー用解放設定")]
    public GameObject entranceBlocker;
    public GameObject entranceBlockerR;
    public GameObject bossCameraObj;

    [Header("ステージボス用設定")]
    public GoalPoint stageGoalPoint;

    [Header("撃破エフェクト設定")]
    public GameObject deathParticlePrefab;

    [Header("攻撃パターン（フェーズ）設定")]
    public EnemyTurret[] phase1Turrets;
    public EnemyTurret[] phase2Turrets;
    [Range(0.1f, 0.9f)] public float phase2Threshold = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isBattleStarted = false;
    private bool isDead = false;
    private bool isPhase2 = false;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start(){
        if (bossHpSlider != null) bossHpSlider.gameObject.SetActive(false);

        EnemyTurret[] allTurrets = GetComponentsInChildren<EnemyTurret>();
        foreach (var t in allTurrets){
            t.enabled = false;
        }

        SetMovementScriptsEnabled(false);
    }

    public void StartBossBattle(){
        if (isBattleStarted) return;
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine(){
        if (bossHpSlider != null){
            bossHpSlider.gameObject.SetActive(true);
            bossHpSlider.maxValue = maxHp;
            bossHpSlider.value = 0;
            bossHpText.text = "0";

            float elapsed = 0f;
            float duration = 1.5f;

            while (elapsed < duration){
                elapsed += Time.deltaTime;
                bossHpSlider.value = Mathf.Lerp(0f, maxHp, elapsed / duration);
                bossHpText.text = bossHpSlider.value.ToString("0");
                yield return null;
            }
            bossHpSlider.value = maxHp;
        }

        currentHp = maxHp;
        isBattleStarted = true;
        isPhase2 = false;

        SetTurretsEnabled(phase1Turrets, true);
        SetMovementScriptsEnabled(true);
    }

    private void SetMovementScriptsEnabled(bool isEnabled){
        MonoBehaviour hover = GetComponent("BossHoverMove") as MonoBehaviour;
        if (hover != null) hover.enabled = isEnabled;

        MonoBehaviour patrol = GetComponent("EnemyPatrol") as MonoBehaviour;
        if (patrol != null) patrol.enabled = isEnabled;

        MonoBehaviour move = GetComponent("EnemyMovement") as MonoBehaviour;
        if (move != null) move.enabled = isEnabled;

        MonoBehaviour teleport = GetComponent("BossTeleportMove") as MonoBehaviour;
        if (teleport != null) teleport.enabled = isEnabled;
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        // ▼▼▼ 修正：無敵状態（isInvincible）の時はダメージ処理をシャットアウトする ▼▼▼
        if (!isBattleStarted || isDead || isInvincible) return;

        currentHp -= damage;

        if (bossHpSlider != null){
            bossHpSlider.value = currentHp;
            bossHpText.text = currentHp.ToString();
        }

        if (anim != null) anim.SetTrigger("Damage");

        if (currentHp <= 0){
            Die();
        }else{
            // 死んでいなければ、フェーズ移行判定と無敵時間の開始を行う
            if (!isPhase2 && currentHp <= (maxHp * phase2Threshold)){
                EnterPhase2();
            }

            // ▼ 追加：無敵状態（点滅）を開始する
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // ▼▼▼ 新規追加：無敵時間の点滅コルーチン ▼▼▼
    private IEnumerator InvincibilityRoutine(){
        isInvincible = true; // 無敵フラグをON

        // ボス本体や子オブジェクトにある SpriteRenderer（画像）をすべて取得
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;

        // 指定した無敵時間が経過するまで、またはボスが死ぬまで点滅を繰り返す
        while (elapsed < invincibilityTime && !isDead){
            // 透明にする
            foreach (var sr in srs){
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 0f);
            }
            yield return new WaitForSeconds(blinkInterval);

            // 元に戻す
            foreach (var sr in srs){
                if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f);
            }
            yield return new WaitForSeconds(blinkInterval);

            elapsed += blinkInterval * 2f;
        }

        // 念のため、最後は確実に不透明（元の状態）に戻す
        foreach (var sr in srs){
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f);
        }

        isInvincible = false; // 無敵フラグをOFF
    }
    // ▲▲▲ 新規追加ここまで ▲▲▲

    private void EnterPhase2(){
        isPhase2 = true;
        SetTurretsEnabled(phase1Turrets, false);
        SetTurretsEnabled(phase2Turrets, true);

        if (anim != null){
            anim.SetBool("isPhase2", true);
        }
    }

    private void SetTurretsEnabled(EnemyTurret[] turrets, bool isEnabled){
        if (turrets == null) return;
        foreach (EnemyTurret t in turrets){
            if (t != null) t.enabled = isEnabled;
        }
    }

    public void Shoot(){
        if (!isBattleStarted || isDead) return;
    }

    private void Die(){
        isDead = true;
        isBattleStarted = false;

        if (bossHpSlider != null) bossHpSlider.gameObject.SetActive(false);

        SetTurretsEnabled(phase1Turrets, false);
        SetTurretsEnabled(phase2Turrets, false);
        SetMovementScriptsEnabled(false);

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine(){
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (Collider2D col in colliders){
            if (col.isTrigger) col.enabled = false;
        }

        if (anim != null){
            anim.SetBool("Die", true);
            anim.Play("Damage");
        }

        if (rb != null){
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3f;
        }

        yield return new WaitForSeconds(0.5f);

        if (deathParticlePrefab != null){
            Vector3 effectPos = transform.position + new Vector3(0, 0, -1f);
            Instantiate(deathParticlePrefab, effectPos, Quaternion.identity);
        }

        if (rb != null){
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        if (bossType == BossType.RoomGuarder){
            if (entranceBlocker != null) entranceBlocker.SetActive(false);
            if (entranceBlockerR != null) entranceBlockerR.SetActive(false);
            if (bossCameraObj != null) bossCameraObj.SetActive(false);

            Destroy(gameObject, 0.5f);
        }else if (bossType == BossType.StageBoss){
            yield return new WaitForSeconds(2.0f);
            if (stageGoalPoint != null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) stageGoalPoint.TriggerGoal(playerObj);
            }
        }
    }
}