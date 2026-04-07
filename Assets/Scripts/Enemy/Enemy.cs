using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDamageable{
    public int hp = 3;
    public float knockbackForce = 5f;
    public float knockbackTime = 0.2f; // ノックバックで硬直する時間

    [Header("ドロップ設定")]
    public GameObject itemPrefab; // 落としたいアイテムのプレハブ
    [Range(0, 100)] public int dropChance = 50; // ドロップ率（％）

    private Rigidbody2D rb;
    private EnemyPatrol patrolScript;  // 歩行スクリプトを取得するための変数

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        patrolScript = GetComponent<EnemyPatrol>(); // アタッチされているEnemyPatrolを取得
    }

    public void TakeDamage(int damage, Vector2 knockbackDirection){
        hp -= damage;

        // ノックバック処理
        rb.linearVelocity = Vector2.zero;

        // Y軸の0.5fを0fにすると、上に浮かず真横に飛びます（お好みで調整してください）
        Vector2 force = new Vector2(knockbackDirection.x, 0f).normalized * knockbackForce;
        rb.AddForce(force, ForceMode2D.Impulse);

        if (hp <= 0){
            Die();
        }else{
            // ダメージエフェクトと硬直（ノックバック）のコルーチンを開始
            StartCoroutine(DamageRoutine());
        }
    }

    private void Die(){
        // 確率判定
        if (Random.Range(0, 100) < dropChance && itemPrefab != null){
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private IEnumerator DamageRoutine(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // 1. 歩行スクリプトをオフにして、強制的な速度上書きを止める
        if (patrolScript != null) patrolScript.enabled = false;

        // 色を白にしてダメージを表現
        sr.color = Color.white;

        // 2. ノックバックしている時間（knockbackTime）だけ待機
        yield return new WaitForSeconds(knockbackTime);

        // 3. 元に戻す
        sr.color = Color.red;
        if (patrolScript != null) patrolScript.enabled = true; // 歩行再開
    }
}