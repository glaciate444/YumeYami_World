/* ===================================================
 * スクリプト名 : 弾スクリプト
 * Version : Ver0.01
 * Update : 2026/04/09
 * 用途 : 弾のスクリプト、味方敵共通
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour{
    [Header("弾の設定")]
    public float speed = 15f;
    public int damage = 2;
    public float lifeTime = 2f;

    [Header("同士討ち防止")]
    public string ignoreTag = "Player"; // インスペクターで設定可能にする

    private Rigidbody2D rb;

    public void Initialize(Vector2 direction){
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision){
        // 撃った本人（無視するタグ）や、カメラ枠なら何もしない
        if (collision.CompareTag(ignoreTag)) return;
        if (collision.gameObject.name == "CameraBounds") return;

        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null){
            Vector2 knockbackDir = rb.linearVelocity.normalized;
            target.TakeDamage(damage, knockbackDir);
        }

        Destroy(gameObject);
    }
}