/* ===================================================
 * スクリプト名 : RollingHazard.cs
 * 用途 : 物理演算で転がり、プレイヤーを狙う障害物
 * 更新 : 着地・激突時の効果音（衝撃検知）を追加
 * =================================================== */
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class RollingHazard : MonoBehaviour{
    [Header("ダメージ設定")]
    public int damage = 2;

    [Header("初速設定")]
    public float initialForce = 3f;
    public float initialTorque = 5f;

    [Header("消滅設定")]
    public float lifeTime = 10f; 

    // ▼【追加】効果音の設定
    [Header("効果音設定")]
    public AudioClip impactSE; // 着地・激突した時の「ドスッ」という音
    [Tooltip("どのくらいの強さでぶつかったら音を鳴らすか（小さすぎるとコロコロ転がるだけで鳴ります）")]
    public float impactThreshold = 2.0f; 

    private Rigidbody2D rb;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null){
            float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
            rb.AddForce(new Vector2(direction * initialForce, 0f), ForceMode2D.Impulse);
            rb.AddTorque(-direction * initialTorque, ForceMode2D.Impulse);
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter2D(Collision2D other){
        // ▼【追加】何かにぶつかった時の「衝撃の強さ」を計算して音を鳴らす ▼
        // other.relativeVelocity.magnitude が、ぶつかった瞬間のスピード（衝撃）です
        if (other.relativeVelocity.magnitude > impactThreshold){
            if (SoundManager.instance != null && impactSE != null){
                SoundManager.instance.PlaySE(impactSE);
            }
        }

        // ▼ 以下は元のプレイヤーへのダメージ処理 ▼
        if (other.gameObject.CompareTag("Player")){
            IDamageable target = other.gameObject.GetComponent<IDamageable>();
            if (target != null){
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = Mathf.Max(knockbackDir.y, 0.5f); 
                target.TakeDamage(damage, knockbackDir);
            }
        }
    }
}