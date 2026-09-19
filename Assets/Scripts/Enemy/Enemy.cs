/* ===================================================
* スクリプト名 : Enemy.cs
* 用途 : 敵のステータス管理（アニメーション対応版）
* 拡張 : 死亡・ノックバック時、自身に付いている全スクリプトを自動検知して停止するよう改修
* =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable {
    public int hp = 3;
    public float knockbackTime = 0.2f;

    [Header("無敵設定")]
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
    private Animator anim;
    
    // ▼ 変更：1つだけでなく、付いているすべての移動スクリプトを配列で管理する
    private EnemyMovement[] movementScripts; 

    // ▼ 変更：外（EnemyActivatorなど）から生死を確認できるように public に変更
    public bool isDead { get; private set; } = false;

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        // ▼ 自分に付いている「EnemyMovementを継承したスクリプト」を根こそぎ取得する
        movementScripts = GetComponents<EnemyMovement>(); 
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        if (isInvincible || isDead) return;

        hp -= damage;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(knockbackDirection.x, 0f);
        rb.AddForce(force, ForceMode2D.Impulse);

        if (anim != null) anim.SetTrigger("Damage");

        if (hp <= 0){
            Die();
        }else{
            StartCoroutine(DamageRoutine());
        }
    }

    private void Die(){
        isDead = true;
        StopAllCoroutines();

        if (explosionEffectPrefab != null){
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        if (Random.Range(0, 100) < dropChance && itemPrefab != null){
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(ComicalDeathRoutine());
    }

    private IEnumerator ComicalDeathRoutine(){
        // ▼▼▼ 大幅改修：自分（Enemy.cs）以外のすべてのスクリプトを問答無用で完全停止する ▼▼▼
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (var script in allScripts){
            if (script != this) script.enabled = false;
        }

        // 2. 当たり判定を全て消す
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        foreach (var col in colliders){
            col.enabled = false;
        }

        // 3. アニメーションを固定
        if (anim != null){
            anim.Play("Damage");
            anim.speed = 0f;
        }

        // 4. マリオのように、少し上に跳ねてから画面下に落ちる
        if (rb != null){
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 4f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.linearVelocity = new Vector2(0f, deathJumpForce);
        }

        // 5. くるくる回転
        float timer = 3f;
        while (timer > 0f){
            transform.Rotate(0, 0, deathSpinSpeed * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator DamageRoutine(){
        // ▼ 変更：見つけたすべての移動スクリプトを一時停止する
        if (movementScripts != null){
            foreach (var m in movementScripts) m.PauseMovement(true);
        }

        yield return new WaitForSeconds(knockbackTime);

        // ▼ 変更：見つけたすべての移動スクリプトを再開する
        if (movementScripts != null){
            foreach (var m in movementScripts) m.PauseMovement(false);
        }
    }
}