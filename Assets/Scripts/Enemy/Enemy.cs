/* ===================================================
 * スクリプト名 : Enemy.cs
 * Version : Ver0.03
 * Since : 2026/04/09
 * Update : 2026/05/07
 * 用途 : 敵のステータス管理（継承対応版）
 * =================================================== */
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable {
    public int hp = 3;
    public float knockbackForce = 5f;
    public float knockbackTime = 0.2f; 

    [Header("ドロップ設定")]
    public GameObject itemPrefab; 
    [Range(0, 100)] public int dropChance = 50; 
    
    [Header("演出")]
    public GameObject explosionEffectPrefab; 

    private Rigidbody2D rb;

    // ▼ 【変更】EnemyPatrolではなく、基底クラスである EnemyMovement を取得する
    private EnemyMovement movementScript; 

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        // アタッチされている「EnemyMovementを継承した何らかのスクリプト」を自動で探す
        movementScript = GetComponent<EnemyMovement>(); 
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        hp -= damage;
        rb.linearVelocity = Vector2.zero;

        Vector2 force = new Vector2(knockbackDirection.x, 0f).normalized * knockbackForce;
        rb.AddForce(force, ForceMode2D.Impulse);

        if (hp <= 0){
            Die();
        }else{
            StartCoroutine(DamageRoutine());
        }
    }

    private void Die(){
        if (explosionEffectPrefab != null){
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        if (Random.Range(0, 100) < dropChance && itemPrefab != null){
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private IEnumerator DamageRoutine(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // ▼ 【変更】どんな動きの敵であっても、共通の命令で動きを停止させる
        if (movementScript != null) movementScript.PauseMovement(true);

        sr.color = Color.white;
        yield return new WaitForSeconds(knockbackTime);
        sr.color = Color.red;

        // ▼ 【変更】ノックバックが終わったら動きを再開させる
        if (movementScript != null) movementScript.PauseMovement(false);
    }
}