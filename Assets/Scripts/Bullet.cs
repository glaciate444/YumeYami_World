/* ===================================================
 * スクリプト名 : 発射台スクリプト
 * Version : Ver0.01
 * Update : 2026/04/09
 * 用途 : 発射台
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour{
    [Header("弾の設定")]
    public float speed = 15f;
    public int damage = 2;
    public float lifeTime = 2f; // 何秒後に消滅するか

    private Rigidbody2D rb;

    // 発射時にPlayerから呼ばれる初期化メソッド
    public void Initialize(Vector2 direction){
        rb = GetComponent<Rigidbody2D>();
        // Unity 6の仕様で速度を代入
        rb.linearVelocity = direction * speed;

        // 指定時間後に自身を削除（画面外へ飛んでいった時のメモリ対策）
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        // プレイヤー自身には当たらないようにする
        if (other.CompareTag("Player")) return;

        // 相手がダメージを受けられるか確認
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null){
            // 弾の進行方向へノックバックさせる
            Vector2 knockbackDir = rb.linearVelocity.normalized;
            target.TakeDamage(damage, knockbackDir);
        }

        // 壁や敵に当たったら弾を消す
        Debug.Log("弾が当たって消えた相手は： " + other.gameObject.name);

        Destroy(gameObject);
    }
}