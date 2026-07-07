/* ===================================================
* スクリプト名 : Enemy.cs
* Version : Ver0.07
* Since : 2026/04/09
* Update : 2026/07/07
* 用途 : 敵のステータス管理（アニメーション対応版）
* 拡張 : くるくる落下中にEnemyActivatorが干渉するバグを修正
* =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable {
    public int hp = 3;
    public float knockbackTime = 0.2f;

    [Header("無敵設定")]
    [Tooltip("チェックを入れると、プレイヤーからの攻撃を一切受け付けなくなります")]
    public bool isInvincible = false;

    [Header("ドロップ設定")]
    public GameObject itemPrefab;
    [Range(0, 100)] public int dropChance = 50;

    [Header("演出")]
    public GameObject explosionEffectPrefab;

    [Header("コミカル撃破設定（落下＆回転）")]
    public float deathJumpForce = 5f;
    public float deathSpinSpeed = 1000f;

    private Rigidbody2D rb;
    private EnemyMovement movementScript;
    private Animator anim;

    private bool isDead = false;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        movementScript = GetComponent<EnemyMovement>();
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        if (isInvincible || isDead){
            return;
        }

        hp -= damage;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(knockbackDirection.x, 0f);
        rb.AddForce(force, ForceMode2D.Impulse);

        if (anim != null){
            anim.SetTrigger("Damage");
        }

        if (hp <= 0){
            Die();
        }else{
            StartCoroutine(DamageRoutine());
        }
    }

    private void Die(){
        isDead = true;

        if (explosionEffectPrefab != null){
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        if (Random.Range(0, 100) < dropChance && itemPrefab != null){
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(ComicalDeathRoutine());
    }

    // ==========================================
    // ▼ コミカル撃破コルーチン
    // ==========================================
    private IEnumerator ComicalDeathRoutine(){
        // 1. 通常の移動スクリプトを止める
        if (movementScript != null) movementScript.enabled = false;

        // ▼【超重要・新規追加】画面外でリスポーン（復活）させようとするスクリプトを止める！
        EnemyActivator activator = GetComponent<EnemyActivator>();
        if (activator != null) activator.enabled = false;

        // 2. 当たり判定を全て消す
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders){
            col.enabled = false;
        }

        // 3. アニメーションを「Damage」に固定する
        if (anim != null){
            anim.Play("Damage");
        }

        // 4. マリオのように、少し上に跳ねてから画面下に落ちる物理設定
        if (rb != null){
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 4f;
            rb.linearVelocity = new Vector2(0f, deathJumpForce);
        }

        // 5. くるくる回転させながら落下を待つ
        float timer = 3f;
        while (timer > 0f){
            transform.Rotate(0, 0, deathSpinSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        // 6. 完全にシーンから消去
        Destroy(gameObject);
    }

    private IEnumerator DamageRoutine(){
        if (movementScript != null) movementScript.PauseMovement(true);
        yield return new WaitForSeconds(knockbackTime);
        if (movementScript != null) movementScript.PauseMovement(false);
    }
}