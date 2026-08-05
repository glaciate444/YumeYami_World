/* ===================================================
 * スクリプト名 : 弾スクリプト
 * Version : Ver0.03
 * Since : 2026/04/09
 * Update : 貫通弾（壁や敵をすり抜ける）の機能を追加
 * 用途 : 弾のスクリプト、味方敵共通、ノックバック設定
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour {
    [Header("弾の設定")]
    public float speed = 15f;
    public int damage = 2;         // 「ダメージの値」
    public float impact = 5f;      // 「衝撃」

    public float lifeTime = 2f;

    // ▼▼▼ 新規追加：貫通フラグ ▼▼▼
    [Tooltip("チェックを入れると、壁や敵に当たっても消滅せずに貫通します")]
    public bool isPiercing = false;
    // ▲▲▲ 新規追加ここまで ▲▲▲

    [Header("同士討ち防止")]
    public string ignoreTag = "Player"; // インスペクターで設定可能にする

    private Rigidbody2D rb;

    [Header("エフェクト")]
    public GameObject hitEffectPrefab; // 先ほど作った HitEffect プレハブをセットする

    public void Initialize(Vector2 direction){
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag(ignoreTag)) return;
        if (other.gameObject.name == "CameraBounds") return;

        // ▼【追加】消滅する直前にエフェクトを生成する ▼
        if (hitEffectPrefab != null){
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // ダメージ処理...
        IDamageable target = other.GetComponent<IDamageable>();
        if (target != null){
            Vector2 knockbackForce = rb.linearVelocity.normalized * impact;
            target.TakeDamage(damage, knockbackForce);
        }

        // ▼▼▼ 新規追加：貫通フラグがONなら、ここで処理を終えて弾を飛ばし続ける ▼▼▼
        if (isPiercing) return;
        // ▲▲▲ 新規追加ここまで ▲▲▲

        Destroy(gameObject); // 貫通フラグがOFFの時のみ、弾自身を消滅させる
    }
}